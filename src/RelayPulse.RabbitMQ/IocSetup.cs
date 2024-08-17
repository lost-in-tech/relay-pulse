using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class IocSetup
{
    public static IServiceCollection AddRabbitMqRelayPulse(this IServiceCollection services,
        IConfiguration configuration,
        RabbitMqRelayHubOptions? options = null)
    {
        options ??= new RabbitMqRelayHubOptions();

        var settings = MergeSettings(configuration, options);
        
        services.TryAddSingleton<IRabbitMqConnectionSettings>(settings);
        services.TryAddSingleton<IMessagePublishSettings>(settings);
        services.TryAddSingleton<IPublisherChannelSettings>(settings);
        
        services.TryAddSingleton<BasicPropertiesBuilder>();
        services.TryAddSingleton<IUniqueId, UniqueId>();
        services.TryAddSingleton<IMessagePublisher, MessagePublisher>();
        
        services.TryAddSingleton<IRabbitMqConnectionInstance, RabbitMqConnectionInstance>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChannelFactory,PublisherPerTypeChannelFactory>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IChannelFactory,PublisherDefaultChannelFactory>());
        
        services.TryAddSingleton<IRabbitMqWrapper, RabbitMqWrapper>();
        services.TryAddSingleton<IMessageSerializer,MessageSerializer>();

        return services;
    }

    private static RabbitMqSettings MergeSettings(IConfiguration configuration, RabbitMqRelayHubOptions options)
    {
        var config = new RabbitMqSettings();
        configuration.GetSection(options.ConfigSectionName).Bind(config);

        return new RabbitMqSettings
        {
            Uri = config.Uri.TryPickNonEmpty(options.Settings?.Uri).NullToEmpty(),
            DefaultExchange = config.DefaultExchange.TryPickNonEmpty(options.Settings?.DefaultExchange).NullToEmpty(),
            TypePrefix = config.TypePrefix.TryPickNonEmpty(options.Settings?.TypePrefix)
        };
    }
}

public record RabbitMqRelayHubOptions
{
    public string ConfigSectionName { get; init; } = "RelayPulse.RabbitMQ";
    public RabbitMqSettings? Settings { get; init; }
}