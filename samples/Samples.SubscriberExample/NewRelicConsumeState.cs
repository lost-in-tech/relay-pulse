using NewRelic.Api.Agent;
using RelayPulse.Core;

namespace Samples.SubscriberExample;


public class NewRelicConsumeState : INotifyConsumeState
{
    private static readonly IAgent _agent = NewRelic.Api.Agent.NewRelic.GetAgent();
    public Task Received(ConsumerInput input, CancellationToken ct = default)
    {
        NewRelic.Api.Agent.NewRelic.RecordCustomEvent($"{input.Queue}/received", Enumerable.Empty<KeyValuePair<string,object>>());
        _agent.CurrentTransaction.AddCustomAttribute($"{input.Queue}/received/count", 1);
        return Task.CompletedTask;
    }

    public Task Processed(ConsumerInput input, ConsumerResponse response, CancellationToken ct = default)
    {
        NewRelic.Api.Agent.NewRelic.RecordCustomEvent($"{input.Queue}/processed", Enumerable.Empty<KeyValuePair<string,object>>());
        _agent.CurrentTransaction.AddCustomAttribute($"{input.Queue}/processed/{response.Status}", 1);
        _agent.CurrentTransaction.AddCustomAttribute($"{input.Queue}/processed/{response.Status}/{response.Reason}", 1);
        return Task.CompletedTask;
    }
}