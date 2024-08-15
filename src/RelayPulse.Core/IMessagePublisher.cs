namespace RelayPulse.Core;

public interface IMessagePublisher
{
    Task<bool> Publish<T>(Message<T> msg, CancellationToken ct = default);
    Task<bool> Publish<T>(T content, CancellationToken ct = default);
}