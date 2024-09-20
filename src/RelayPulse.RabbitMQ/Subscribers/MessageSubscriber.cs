using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class MessageSubscriber(
    IServiceProvider sp,
    IMessageSerializer serializer,
    ITraceKeySettings traceKeySettings,
    IQueueSettings queueSettings,
    IRabbitMqWrapper wrapper,
    ILogger<MessageSubscriber> logger)
{
    public async Task Subscribe(IModel channel, QueueInfo queueInfo, BasicDeliverEventArgs args, CancellationToken ct)
    {
        logger.LogTrace("Start broadcasting message received in queue {queue} with {msgId} {publisherAppId} {cid}",
            queueInfo.Name,
            args.BasicProperties.MessageId,
            args.BasicProperties.AppId,
            args.BasicProperties.CorrelationId);

        ConsumerInput? input = null;
        
        using var scope = sp.CreateScope();

        var notifier = scope.ServiceProvider.GetRequiredService<NotifyConsumeStateWrapper>();
        
        try
        {
            input = ConsumerInputBuilder.Build(queueInfo, args);
            
            var traceContextProvider = scope.ServiceProvider.GetRequiredService<ITraceContextWriter>();

            var traceContext = new TraceContext
            {
                Queue = input.Queue,
                Tenant = input.Tenant,
                AppId = queueSettings.AppId,
                ConsumerId = input.AppId,
                TraceId = input.TraceId,
                UserId = input.UserId,
                MsgId = input.Id,
                RetryCount = input.RetryCount,
                SentAt = input.SentAt
            };
            
            traceContextProvider.Set(traceContext);

            await notifier.Received(input, ct);

            using var logScope = CreateLogScope(traceContext);
            
            var processors = scope.ServiceProvider.GetServices<IMessageConsumer>();

            var consumer = processors.FirstOrDefault(x => x.IsApplicable(input));

            if (consumer == null)
            {
                await HandleNoConsumer(channel, queueInfo, args, ct, notifier, input);

                return;
            }

            var bodyArray = args.Body.ToArray();

            await ExecuteConsumer(channel, queueInfo, args, ct, bodyArray, consumer, input, notifier);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Processing of message failed with {error} {msgId}", e.Message, args.BasicProperties.MessageId);

            channel.BasicReject(args.DeliveryTag, false);

            await notifier.Processed(input ?? new ConsumerInput
            {
                Id = args.BasicProperties.MessageId
            }, ConsumerResponse.PermanentFailure("UnhandledError"), ct);
        }
    }

    private IDisposable? CreateLogScope(ITraceContextDto context)
    {
        return logger.BeginScope(new Dictionary<string, object>()
        {
            [traceKeySettings.AppIdLogKey ?? Constants.AppIdLogKey] = context.AppId ?? string.Empty,
            [traceKeySettings.TenantLogKey ?? Constants.TenantLogKey] = context.Tenant ?? string.Empty,
            [traceKeySettings.TraceIdLogKey ?? Constants.TraceIdLogKey] = context.TraceId ?? string.Empty,
            [traceKeySettings.ConsumerIdLogKey ?? Constants.ConsumerIdLogKey] = context.ConsumerId ?? string.Empty,
            [traceKeySettings.MessageIdLogKey ?? Constants.MessageIdLogKey] = context.MsgId ?? string.Empty,
            [traceKeySettings.UserIdLogKey ?? Constants.UserIdLogKey] = context.UserId ?? string.Empty,
            [traceKeySettings.QueueLogKey ?? Constants.QueueLogKey] = context.Queue ?? string.Empty
        });
    }

    private async Task HandleNoConsumer(IModel channel, QueueInfo queueInfo, BasicDeliverEventArgs args,
        CancellationToken ct, NotifyConsumeStateWrapper notifier, ConsumerInput input)
    {
        logger.LogError("No consumer available to process this message.");

        if (queueInfo.DeadLetterExchange.HasValue())
        {
            logger.LogError(
                "Rejecting the message as no consumer available to process. Should move to dead letter queue");

            channel.BasicReject(args.DeliveryTag, false);
        }
        else
        {
            logger.LogError(
                "No consumer available to process the message. As no dead letter exchange active so nack and requeue the message");

            channel.BasicNack(args.DeliveryTag, false, true);
        }

        await notifier.Processed(input, ConsumerResponse.PermanentFailure("NoConsumerDefined"), ct);
    }

    private async Task ExecuteConsumer(IModel channel, 
        QueueInfo queueInfo, 
        BasicDeliverEventArgs args,
        CancellationToken ct, 
        byte[] bodyArray, 
        IMessageConsumer consumer, 
        ConsumerInput input,
        NotifyConsumeStateWrapper notifier)
    {
        using var ms = new MemoryStream(bodyArray);

        var rsp = await consumer.Consume(input, ms, serializer, ct);
            
        if (rsp.Status == MessageProcessStatus.Success)
        {
            channel.BasicAck(args.DeliveryTag, false);
        }
        else if (rsp.Status == MessageProcessStatus.PermanentFailure)
        {
            channel.BasicReject(args.DeliveryTag, false);
        }
        else
        {
            var retryAfterInSeconds = (int)(rsp.RetryAfter?.TotalSeconds 
                                            ?? queueInfo.DefaultRetryAfterInSeconds 
                                            ?? 60);
            
            var shouldMoveToRetryQueue = queueInfo.RetryExchange.HasValue() 
                                         && queueInfo.DeadLetterExchange.HasValue();

            if (shouldMoveToRetryQueue && retryAfterInSeconds > 10)
            {
                args.BasicProperties.Headers ??= new Dictionary<string, object>();

                args.BasicProperties.Expiration  = (retryAfterInSeconds * 1000).ToString("F0");

                var retryCount = args.BasicProperties.Headers.RetryCount();
                args.BasicProperties.Headers.RetryCount(retryCount + 1);

                wrapper.BasicPublish(channel, new BasicPublishInput
                {
                    BasicProperties = args.BasicProperties,
                    Body = args.Body,
                    Exchange = queueInfo.DeadLetterExchange!,
                    RoutingKey = queueInfo.Name
                });

                channel.BasicAck(args.DeliveryTag, false);
            }
            else
            {
                if (retryAfterInSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterInSeconds), ct);
                }
                
                channel.BasicReject(args.DeliveryTag, true);
            }
        }

        await notifier.Processed(input, rsp, ct);
    }
}