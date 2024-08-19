using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal sealed class MessageListener(
    SetupRabbitMq setupRabbitMq,
    QueueSettingsValidator validator,
    IQueueSettings settings,
    IChannelFactory channelFactory) : IMessageListener
{
    public Task Listen(CancellationToken ct)
    {
        validator.Validate(settings);
        
        var queues = setupRabbitMq.Run(settings);

        foreach (var queue in queues)
        {
            var channel = channelFactory.GetOrCreate(queue.Name);

            channel.QueueDeclarePassive(queue.Name);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, args) =>
            {
                Console.WriteLine(args.BasicProperties.MessageId);
            };
        }
        
        return Task.CompletedTask;
    }
}