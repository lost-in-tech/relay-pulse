namespace RelayPulse.Core;

public interface IMessagePublisher
{
    Task<bool> Publish<T>(Message<T> msg, CancellationToken ct);
    Task<bool> Publish<T>(T content, CancellationToken ct);
}