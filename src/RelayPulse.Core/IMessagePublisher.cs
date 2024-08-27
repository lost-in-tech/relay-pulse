namespace RelayPulse.Core;

public interface IMessagePublisher
{
    Task<MessagePublishResponse> Publish<T>(Message<T> msg, CancellationToken ct = default);
    Task<MessagePublishResponse> Publish<T>(T content, CancellationToken ct = default);
}

public record MessagePublishResponse
{
    public Guid Id { get; set; }
}