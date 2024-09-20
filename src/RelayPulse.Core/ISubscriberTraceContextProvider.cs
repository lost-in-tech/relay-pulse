namespace RelayPulse.Core;

public interface ISubscriberTraceContextProvider
{
    ITraceContextDto Get();
}

public interface ITraceContextDto
{
    string? TraceId { get; }
    string? AppId { get; }
    string? ConsumerId { get; }
    string? Tenant { get; }
    string? UserId { get; }
    string? Queue { get; }
    
    string? MsgId { get; set; }
    
    DateTime? SentAt { get; set; }
    
    int? RetryCount { get; set; }
}

