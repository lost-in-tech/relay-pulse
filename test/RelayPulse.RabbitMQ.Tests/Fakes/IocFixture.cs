using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;

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

        sc.Replace(ServiceDescriptor.Singleton<IClockWrap>(_ =>
        {
            var clock = Substitute.For<IClockWrap>();
            clock.UtcNow.Returns(new DateTime(2024, 08, 20, 0, 0, 0));
            return clock;
        }));
        sc.Replace(ServiceDescriptor.Singleton<IUniqueId, FakeUniqueId>());
        sc.Replace(ServiceDescriptor.Singleton<IRabbitMqWrapper, FakeRabbitMqWrapper>());
        sc.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionInstance, FakeRabbitMqConnectionInstance>());
        sc.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(FakeLogger<>)));
        return sc.BuildServiceProvider();
    }
}