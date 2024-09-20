namespace RelayPulse.Core;

public interface IMessageConsumer
{
    Task<ConsumerResponse> Consume(
        ConsumerInput input,
        Stream content,
        IMessageSerializer serializer,
        CancellationToken ct);
    
    bool IsApplicable(ConsumerInput input);
}

public abstract class MessageConsumer<T> : IMessageConsumer
{
    Task<ConsumerResponse> IMessageConsumer.Consume(
        ConsumerInput input,
        Stream content,
        IMessageSerializer serializer,
        CancellationToken ct)
    {
        var cnt = serializer.Deserialize<T>(content);
        
        return Consume(new ConsumerInput<T>(cnt)
        {
            Queue = input.Queue,
            TraceId = input.TraceId,
            Headers = input.Headers,
            Id = input.Id,
            Tenant = input.Tenant,
            Type = input.Type,
            AppId = input.AppId,
            SentAt = input.SentAt,
            UserId = input.UserId,
            RetryCount = input.RetryCount
        }, ct);
    }

    protected abstract Task<ConsumerResponse> Consume(ConsumerInput<T> input, CancellationToken ct);
    
    public abstract bool IsApplicable(ConsumerInput input);
}

public record ConsumerInput
{
    public string Queue { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string? AppId { get; set; }
    public string? TraceId { get; set; }
    public string? UserId { get; set; }
    public string? Type { get; set; }
    public string? Tenant { get; set; }
    public DateTime? SentAt { get; set; }
    public int? RetryCount { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public record ConsumerInput<T> : ConsumerInput
{
    public ConsumerInput(T content)
    {
        Content = content;
    }
    
    public ConsumerInput(ConsumerInput input, T content)
    {
        Content = content;
        Queue = input.Queue;
        Type = input.Type;
        TraceId = input.TraceId;
        Id = input.Id;
        Tenant = input.Tenant;
        AppId = input.AppId;
        UserId = input.UserId;
        SentAt = input.SentAt;
        RetryCount = input.RetryCount;
        Headers = input.Headers;
    }
    
    public T Content { get; set; }
}

public record ConsumerResponse
{
    private ConsumerResponse(MessageProcessStatus status)
    {
        Status = status;
    }
    
    public MessageProcessStatus Status { get; set; }
    public TimeSpan? RetryAfter { get; set; }
    public string? Reason { get; set; }

    public static ConsumerResponse Success() => new(MessageProcessStatus.Success);
    public static ConsumerResponse PermanentFailure(string reason) => new(MessageProcessStatus.PermanentFailure){ Reason = reason };
    public static ConsumerResponse TransientFailure(string reason) => new(MessageProcessStatus.TransientFailure){ Reason = reason };
    public static ConsumerResponse TransientFailure(string reason, TimeSpan retryAfter) => new(MessageProcessStatus.TransientFailure)
    {
        Reason = reason,
        RetryAfter = retryAfter
    };
}

public enum MessageProcessStatus
{
    Success,
    PermanentFailure,
    TransientFailure
}