namespace RelayPulse.Core;


public interface ITraceKeySettings
{
    string? AppIdHttpHeaderName { get; }
    string? TraceIdHttpHeaderName { get; }
    string? AppIdLogKey { get; }
    string? TraceIdLogKey { get; }
    string? UserIdLogKey { get; }
    string? TenantLogKey { get; }
    string? ConsumerIdLogKey { get; }
    string? QueueLogKey { get; }
    string? MessageIdLogKey { get; }
}