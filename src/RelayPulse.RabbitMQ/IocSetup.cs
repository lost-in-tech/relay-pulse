using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RelayPulse.Core;
using RelayPulse.RabbitMQ.Publishers;
using RelayPulse.RabbitMQ.Subscribers;

namespace RelayPulse.RabbitMQ;

public static class IocSetup
{
    public static IServiceCollection AddRabbitMqRelayPulse(this IServiceCollection services,
        IConfiguration configuration,
        RabbitMqRelayHubOptions? options = null)
    {
        options ??= new RabbitMqRelayHubOptions();

        services.Configure<RabbitMqSettings>(configuration.GetSection(options.ConfigSectionName));

        services.TryAddSingleton<IRabbitMqConnectionSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IMessagePublishSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IPublisherChannelSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IQueueSettings>(sc => MergeSettings(sc, options));
        
        services.TryAddSingleton<BasicPropertiesBuilder>();
        services.TryAddSingleton<IUniqueId, UniqueId>();
        services.TryAddSingleton<IClockWrap,ClockWrap>();
        services.TryAddSingleton<IMessagePublisher, MessagePublisher>();
        
        services.TryAddSingleton<IRabbitMqConnectionInstance, RabbitMqConnectionInstance>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChannelFactory,PublisherPerTypeChannelFactory>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChannelFactory,PublisherDefaultChannelFactory>());
        
        services.TryAddSingleton<IRabbitMqWrapper, RabbitMqWrapper>();
        services.TryAddSingleton<IMessageSerializer,MessageSerializer>();
        
        // Subscribers
        services.TryAddSingleton<QueueSettingsValidator>();
        services.TryAddSingleton<SetupRabbitMq>();
        services.TryAddSingleton<IMessageListener, MessageListener>();
        services.TryAddSingleton<MessageSubscriber>();
        
        return services;
    }

    private static RabbitMqSettings MergeSettings(IConfiguration configuration, RabbitMqRelayHubOptions options)
    {
        var config = new RabbitMqSettings();
        configuration.GetSection(options.ConfigSectionName).Bind(config);

        return SettingsMerger.Read(options.Settings, config);
    }
    
    private static RabbitMqSettings MergeSettings(IServiceProvider sp, RabbitMqRelayHubOptions options)
    {
        var config = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

        return MergeSettings(config, options);
    }
    
    private static RabbitMqSettings MergeSettings(RabbitMqSettings configSettings, RabbitMqRelayHubOptions options)
    {
        return SettingsMerger.Read(options.Settings, configSettings);
    }
}

public record RabbitMqRelayHubOptions
{
    public string ConfigSectionName { get; init; } = "RelayPulse:RabbitMQ";
    public RabbitMqSettings? Settings { get; init; }
}