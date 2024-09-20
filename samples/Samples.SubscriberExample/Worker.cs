using RelayPulse.Core;

namespace Samples.SubscriberExample;

public class Worker(IMessageListener listener) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await listener.ListenUntilCancelled(stoppingToken);
    }
}