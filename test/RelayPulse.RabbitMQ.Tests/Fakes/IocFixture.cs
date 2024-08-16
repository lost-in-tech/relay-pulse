using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public sealed class IocFixture
{
    public IServiceProvider Services(Action<IServiceCollection>? setup = null)
    {
        var config = new ConfigurationBuilder().Build();
        var sc = new ServiceCollection();
        sc.AddRabbitMqRelayPulse(config, new RabbitMqRelayHubOptions
        {
            Settings = RabbitMqSettingsBuilder.Build()
        });
        
        setup?.Invoke(sc);
        
        sc.Replace(ServiceDescriptor.Singleton<IUniqueId, FakeUniqueId>());
        sc.Replace(ServiceDescriptor.Singleton<IRabbitMqWrapper, FakeRabbitMqWrapper>());
        sc.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionInstance, FakeRabbitMqConnectionInstance>());
        
        return sc.BuildServiceProvider();
    }
}