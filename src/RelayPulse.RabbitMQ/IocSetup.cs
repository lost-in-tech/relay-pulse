using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class IocSetup
{
    public static IServiceCollection AddRabbitMqRelayHub(this IServiceCollection services,
        IConfiguration configuration,
        RabbitMqRelayHubOptions? options = null)
    {
        options ??= new RabbitMqRelayHubOptions();

        services.Configure<RabbitMqSettings>(configuration.GetSection(options.ConfigSectionName));

        
        if (options.Settings == null)
        {
            services.TryAddSingleton<IRabbitMqConnectionSettings>(sc =>
                sc.GetRequiredService<IOptions<RabbitMqSettings>>().Value);
            
            services.TryAddSingleton<IMessagePublishSettings>(sc => 
                sc.GetRequiredService<IOptions<RabbitMqSettings>>().Value);
        }
        else
        {
            services.TryAddSingleton<IMessagePublishSettings>(options.Settings);
            services.TryAddSingleton<IRabbitMqConnectionSettings>(options.Settings);
        }
        
        services.TryAddSingleton<IMessagePublisher, MessagePublisher>();
        services.TryAddSingleton<IChannelInstance, ChannelInstance>();
        services.TryAddSingleton<IRabbitMqWrapper, RabbitMqWrapper>();
        services.TryAddSingleton<IMessageSerializer,MessageSerializer>();
        services.TryAddSingleton<IRabbitMqConnectionInstance, RabbitMqConnectionInstance>();

        return services;
    }
}

public record RabbitMqRelayHubOptions
{
    public string ConfigSectionName { get; init; } = "RelayPulse.RabbitMQ";
    public RabbitMqSettings? Settings { get; init; }
}