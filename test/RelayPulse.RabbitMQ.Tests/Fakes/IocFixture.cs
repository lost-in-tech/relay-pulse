using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public sealed class IocFixture
{
    private readonly IServiceProvider _sp;
    
    public IocFixture()
    {
        var config = new ConfigurationBuilder().Build();
        var sc = new ServiceCollection();
        sc.AddRabbitMqRelayHub(config, new RabbitMqRelayHubOptions
        {
            Settings = new RabbitMqSettings
            {
                Uri = "amqps://guest:guest@localhost/rabbit",
                DefaultExchange = "bookworm.events"
            }
        });

        sc.Replace(ServiceDescriptor.Scoped<IRabbitMqWrapper, FakeRabbitMqWrapper>());
        sc.Replace(ServiceDescriptor.Scoped<IRabbitMqConnectionInstance, FakeRabbitMqConnectionInstance>());
        _sp = sc.BuildServiceProvider();
    }

    public T GetFakeService<T>() where T : class
    {
        var result = _sp.GetRequiredService<T>();
        result.ClearSubstitute();
        result.ClearReceivedCalls();
        return result;
    }

    public RabbitMqPublishCallInfo<T> GetRabbitMqPublishCallInfo<T>()
    {
        return GetRequiredService<IRabbitMqWrapper, FakeRabbitMqWrapper>().GetLastUsedPublishInput<T>();
    }
    
    public T GetRequiredService<T>() where T : class
    {
        return _sp.GetRequiredService<T>();
    }

    public TImpl GetRequiredService<TInterface,TImpl>() where TImpl : TInterface where TInterface : notnull
    {
        return (TImpl)_sp.GetRequiredService<TInterface>();
    }
}