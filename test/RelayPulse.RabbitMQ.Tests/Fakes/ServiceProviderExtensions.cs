using Microsoft.Extensions.DependencyInjection;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public static class ServiceProviderExtensions
{
    public static TImpl GetRequiredService<TInterface,TImpl>(this IServiceProvider sp) where TImpl : TInterface where TInterface : notnull
    {
        return (TImpl)sp.GetRequiredService<TInterface>();
    }
    
    public static RabbitMqPublishCallInfo<T> GetRabbitMqPublishCallInfo<T>(this IServiceProvider sp)
    {
        return sp.GetRequiredService<IRabbitMqWrapper,FakeRabbitMqWrapper>().GetLastUsedPublishInput<T>();
    }
}