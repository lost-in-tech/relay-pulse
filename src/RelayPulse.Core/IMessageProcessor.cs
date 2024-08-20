namespace RelayPulse.Core;

public interface IMessageProcessor
{
    Task<MessageProcessorResponse> Process(
        MessageProcessorInput input,
        Stream content,
        IMessageSerializer serializer,
        CancellationToken ct);
    
    bool IsApplicable(MessageProcessorInput input);
}

public abstract class MessageProcessor<T> : IMessageProcessor
{
    Task<MessageProcessorResponse> IMessageProcessor.Process(
        MessageProcessorInput input,
        Stream content,
        IMessageSerializer serializer,
        CancellationToken ct)
    {
        return Process(new MessageProcessorInput<T>
        {
            Content = serializer.Deserialize<T>(content),
            Queue = input.Queue,
            Cid = input.Cid,
            Headers = input.Headers,
            Id = input.Id,
            Tenant = input.Tenant,
            Type = input.Type,
            AppId = input.AppId,
            SentAt = input.SentAt,
            UserId = input.UserId
        }, ct);
    }

    protected abstract Task<MessageProcessorResponse> Process(MessageProcessorInput<T> input, CancellationToken ct);
    
    public abstract bool IsApplicable(MessageProcessorInput input);
}

public record MessageProcessorInput
{
    public required string Queue { get; init; }
    public string? Id { get; init; }
    public string? AppId { get; init; }
    public string? Cid { get; init; }
    public string? UserId { get; init; }
    public string? Type { get; init; }
    public string? Tenant { get; init; }
    public DateTime? SentAt { get; init; }
    public int? RetryCount { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public record MessageProcessorInput<T> : MessageProcessorInput
{
    public required T Content { get; init; }
}

public record MessageProcessorResponse
{
    private MessageProcessorResponse(MessageProcessStatus status)
    {
        Status = status;
    }
    
    public MessageProcessStatus Status { get; init; }
    public int? RetryAfterInSeconds { get; init; }
    public string? Reason { get; init; }

    public static MessageProcessorResponse Success() => new(MessageProcessStatus.Success);
    public static MessageProcessorResponse PermanentFailure(string reason) => new(MessageProcessStatus.PermanentFailure){ Reason = reason };
    public static MessageProcessorResponse TransientFailure(string reason) => new(MessageProcessStatus.TransientFailure){ Reason = reason };
    public static MessageProcessorResponse TransientFailure(string reason, int retryAfterInSeconds) => new(MessageProcessStatus.TransientFailure)
    {
        Reason = reason,
        RetryAfterInSeconds = retryAfterInSeconds
    };
}

public enum MessageProcessStatus
{
    Success,
    PermanentFailure,
    TransientFailure
}