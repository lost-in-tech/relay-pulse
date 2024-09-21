using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal sealed class AppNameProvider(string name) : IAppNameProvider
{
    public string Get() => name;
}