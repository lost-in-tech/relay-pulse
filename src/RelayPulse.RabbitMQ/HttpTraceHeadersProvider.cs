using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal sealed class HttpTraceHeadersProvider(
    ITraceKeySettings settings,
    ISubscriberTraceContextProvider traceContextProvider) 
    : IHttpTraceHeadersProvider
{
    public IEnumerable<(string Name, string Value)> Get()
    {
        var cxt = traceContextProvider.Get();

        if (cxt.AppId.HasValue() && settings.AppIdHttpHeaderName.HasValue())
        {
            yield return (settings.AppIdHttpHeaderName, cxt.AppId);
        }
        
        if (cxt.TraceId.HasValue() && settings.TraceIdHttpHeaderName.HasValue())
        {
            yield return (settings.TraceIdHttpHeaderName, cxt.TraceId);
        }
    }
}