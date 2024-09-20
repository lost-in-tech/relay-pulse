using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class MessageListener(
    SetupRabbitMq setupRabbitMq,
    QueueSettingsValidator validator,
    IQueueSettings settings,
    IChannelFactory channelFactory,
    MessageSubscriber subscriber,
    ILogger<MessageListener> logger) : IMessageListener, ISetupRabbitMq, IDisposable
{
    private QueueInfo[] _queues = Array.Empty<QueueInfo>();
    private readonly List<IModel> _channels = new();

    private Task Init()
    {
        validator.Validate(settings);
        
        _queues = setupRabbitMq.Run(settings);

        return Task.CompletedTask;
    }

    public async Task Listen(CancellationToken ct)
    {
        await Init();
        
        foreach (var queue in _queues)
        {
            var channel = channelFactory.GetOrCreate(queue.Name);

            channel.QueueDeclarePassive(queue.Name);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                await subscriber.Subscribe(channel, queue, args, ct);
            };

            channel.BasicConsume(queue.Name, false, Guid.NewGuid().ToString(), false, false, null, consumer);

            if (queue.PrefetchCount is > 0)
            {
                channel.BasicQos(0, (ushort)queue.PrefetchCount.Value, false);
            }
            
            _channels.Add(channel);
        }
    }

    public async Task ListenUntilCancelled(CancellationToken ct)
    {
        logger.LogInformation("Start listening.");
        
        await Listen(ct);

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(10000, ct);
            
            logger.LogTrace("Listener listening...");
        }
    }

    public void Dispose()
    {
        foreach (var channel in _channels)
        {
            if (!channel.IsClosed)
            {
                channel.Close();
            }
            
            channel.Dispose();
        }
    }

    Task ISetupRabbitMq.Run(CancellationToken ct)
    {
        return Init();
    }
}