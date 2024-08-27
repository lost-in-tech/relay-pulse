using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class MessageSubscriber(
    IServiceProvider sp,
    IMessageSerializer serializer,
    NotifyConsumeStateWrapper notifier,
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
        
        try
        {
            input = ConsumerInputBuilder.Build(queueInfo, args);

            await notifier.Received(input, ct);

            using var logScope = logger.BeginScope(new Dictionary<string, object>()
            {
                ["tenant"] = input.Tenant ?? string.Empty,
                ["cid"] = input.Cid ?? string.Empty,
                ["publisherAppId"] = input.AppId ?? string.Empty,
                ["msgId"] = input.Id ?? string.Empty,
                ["queue"] = input.Queue
            });

            using var scope = sp.CreateScope();
            var processors = scope.ServiceProvider.GetServices<IMessageConsumer>();

            var consumer = processors.FirstOrDefault(x => x.IsApplicable(input));

            if (consumer == null)
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
                
                return;
            }

            using var ms = new MemoryStream(args.Body.ToArray());

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
                if (queueInfo.RetryExchange.HasValue() && queueInfo.RetryQueue.HasValue())
                {
                    args.BasicProperties.Headers ??= new Dictionary<string, object>();

                    args.BasicProperties.Headers[Constants.HeaderTargetQueue] = queueInfo.Name;
                    args.BasicProperties.Headers.Expiry(rsp.RetryAfter);

                    var retryCount = args.BasicProperties.Headers.RetryCount();
                    args.BasicProperties.Headers.RetryCount(retryCount + 1);

                    channel.BasicPublish(queueInfo.RetryExchange,
                        Constants.RouteKeyTargetQueue(queueInfo.Name),
                        args.BasicProperties,
                        args.Body);

                    channel.BasicAck(args.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(args.DeliveryTag, false, false);
                }
            }

            await notifier.Processed(input, rsp, ct);
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
}