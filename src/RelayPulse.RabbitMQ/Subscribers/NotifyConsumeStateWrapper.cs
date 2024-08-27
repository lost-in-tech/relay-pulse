using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class NotifyConsumeStateWrapper(IEnumerable<INotifyConsumeState> notifiers, ILogger<NotNullWhenAttribute> logger)
{
    public Task Received(ConsumerInput input, CancellationToken ct = default)
    {
        if (!notifiers.Any()) return Task.CompletedTask;
        
        var tasks = new List<Task>();
        
        foreach (var notifier in notifiers)
        {
            tasks.Add(Received(notifier, input, ct));
        }

        return Task.WhenAll(tasks);
    }

    private async Task Received(INotifyConsumeState notifier, ConsumerInput input, CancellationToken ct)
    {
        try
        {
            await notifier.Received(input, ct);
        }
        catch(Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }

    public Task Processed(ConsumerInput input, ConsumerResponse response, CancellationToken ct = default)
    {
        if (!notifiers.Any()) return Task.CompletedTask;
        
        var tasks = new List<Task>();
        
        foreach (var notifier in notifiers)
        {
            tasks.Add(Processed(notifier, input, response, ct));
        }

        return Task.WhenAll(tasks);
    }
    
    private async Task Processed(INotifyConsumeState notifier, ConsumerInput input, ConsumerResponse response, CancellationToken ct = default)
    {
        try
        {
            await notifier.Processed(input, response, ct);
        }
        catch(Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }
}