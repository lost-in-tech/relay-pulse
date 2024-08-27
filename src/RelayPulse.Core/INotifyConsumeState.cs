namespace RelayPulse.Core;

public interface INotifyConsumeState
{
    Task Received(ConsumerInput input, CancellationToken ct = default);
    Task Processed(ConsumerInput input, ConsumerResponse response, CancellationToken ct = default);
}

public class sd : INotifyConsumeState
{
    public Task Received(ConsumerInput input, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task Processed(ConsumerInput input, ConsumerResponse response, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}