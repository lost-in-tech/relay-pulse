using RelayPulse.Core;

namespace Samples.SubscriberExample;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageListener _listener;

    public Worker(ILogger<Worker> logger, IMessageListener listener)
    {
        _logger = logger;
        _listener = listener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _listener.Init(stoppingToken);
        await _listener.Listen(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {

            await Task.Delay(1000, stoppingToken);
        }
    }
}