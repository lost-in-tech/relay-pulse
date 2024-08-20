using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class MessageBroadcaster(
    IServiceProvider sp, 
    IMessageSerializer serializer,
    ILogger<MessageBroadcaster> logger)
{
    public async Task Broadcast(IModel channel, QueueInfo queueInfo, BasicDeliverEventArgs args, CancellationToken ct)
    {
        logger.LogTrace("Start broadcasting message received in queue {queue} with {msgId} {publisherAppId} {cid}", 
            queueInfo.Name, 
            args.BasicProperties.MessageId, 
            args.BasicProperties.AppId,
            args.BasicProperties.CorrelationId);
        
        var input = MessageProcessorInputBuilder.Build(queueInfo, args);

        using var logScope = logger.BeginScope(new Dictionary<string, object>()
        {
            ["tenant"] = input.Tenant ?? string.Empty,
            ["cid"] = input.Cid ?? string.Empty,
            ["srcAppId"] = input.AppId ?? string.Empty,
            ["msgId"] = input.Id ?? string.Empty
        });
        
        using var scope = sp.CreateScope();
        var processors = scope.ServiceProvider.GetServices<IMessageProcessor>();

        var processor = processors.FirstOrDefault(x => x.IsApplicable(input));

        if (processor == null)
        {
            logger.LogError("No processor available to process this message.");

            if (queueInfo.DeadLetterExchange.HasValue())
            {
                logger.LogError("Rejecting the message as no processor available to process. Should move to dead letter queue");
                
                channel.BasicReject(args.DeliveryTag, false);
            }
            else
            {
                logger.LogError("No processor available to process the message. As no dead letter exchange active so nack and requeue the message");
                
                channel.BasicNack(args.DeliveryTag, false, true);
            }
            
            return;
        }

        try
        {
            using var ms = new MemoryStream(args.Body.ToArray());
            
            var rsp = await processor.Process(input, ms, serializer, ct);

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
                    if (args.BasicProperties.Headers == null)
                        args.BasicProperties.Headers = new Dictionary<string, object>();

                    args.BasicProperties.Headers[Constants.HeaderTargetQueue] = queueInfo.Name;
                    args.BasicProperties.Headers.Expiry(rsp.RetryAfterInSeconds);

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
            
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            
            channel.BasicReject(args.DeliveryTag, false);
        }
    }
}