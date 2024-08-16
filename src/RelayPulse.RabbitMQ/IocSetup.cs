using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class IocSetup
{
    public static IServiceCollection AddRabbitMqRelayHub(this IServiceCollection services,
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
            Uri = PickNonEmpty(config.Uri, options.Settings?.Uri),
            DefaultExchange = PickNonEmpty(config.DefaultExchange, options.Settings?.DefaultExchange),
            TypePrefix = PickNonEmpty(config.TypePrefix, options.Settings?.TypePrefix),
            MessageTypeHeaderName = PickNonEmpty(config.MessageTypeHeaderName, options.Settings?.MessageTypeHeaderName)
        };
    }

    private static string PickNonEmpty(string? value, string? alt)
    {
        if (!string.IsNullOrWhiteSpace(value)) return value;

        return alt ?? string.Empty;
    }
}

public record RabbitMqRelayHubOptions
{
    public string ConfigSectionName { get; init; } = "RelayPulse.RabbitMQ";
    public RabbitMqSettings? Settings { get; init; }
}