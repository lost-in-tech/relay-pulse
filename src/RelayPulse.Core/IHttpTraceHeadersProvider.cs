namespace RelayPulse.Core;

public interface IHttpTraceHeadersProvider
{
    public IEnumerable<(string Name, string Value)> Get();
}