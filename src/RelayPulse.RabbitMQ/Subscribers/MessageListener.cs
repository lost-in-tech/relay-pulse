using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class MessageListener(
    SetupRabbitMq setupRabbitMq,
    QueueSettingsValidator validator,
    IQueueSettings settings,
    IChannelFactory channelFactory,
    MessageBroadcaster broadcaster) : IMessageListener, IDisposable
{
    private QueueInfo[] _queues = Array.Empty<QueueInfo>();
    private List<(IModel Channel, AsyncEventingBasicConsumer Consumer)> _consumers = new List<(IModel, AsyncEventingBasicConsumer)>();
    
    public Task Init(CancellationToken ct)
    {
        validator.Validate(settings);
        
        _queues = setupRabbitMq.Run(settings);

        return Task.CompletedTask;
    }

    public Task Listen(CancellationToken ct)
    {
        foreach (var queue in _queues)
        {
            var channel = channelFactory.GetOrCreate(queue.Name);

            channel.QueueDeclarePassive(queue.Name);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                await broadcaster.Broadcast(channel, queue, args, ct);
            };

            channel.BasicConsume(queue.Name, false, Guid.NewGuid().ToString(), false, false, null, consumer);

            if (queue.PrefetchCount is > 0)
            {
                channel.BasicQos(0, (ushort)queue.PrefetchCount.Value, false);
            }
            
            _consumers.Add((channel, consumer));
        }
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var tuple in _consumers)
        {
            if (!tuple.Channel.IsClosed)
            {
                tuple.Channel.Close();
            }
            
            tuple.Channel.Dispose();
        }
    }
}