using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal interface ITraceContextWriter
{
    void Set(ITraceContextDto context);
}

internal sealed class SubscriberTraceContextProvider
    : ISubscriberTraceContextProvider,
        ITraceContextWriter
{
    private ITraceContextDto _context = new TraceContext();
    
    public ITraceContextDto Get()
    {
        return _context;
    }

    public void Set(ITraceContextDto context)
    {
        _context = context;
    }
}



internal record TraceContext : ITraceContextDto
{
    public string? TraceId { get; set; }
    public string? AppId { get; set; }
    public string? ConsumerId { get; set; }
    public string? Tenant { get; set; }
    public string? UserId { get; set; }
    public string? Queue { get; set; }
    
    public string? MsgId { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public int? RetryCount { get; set; }
}