using System.Reflection;
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

        var appId = configuration["appId"] ?? configuration["appName"] ?? configuration["app"];
        
        services.TryAddSingleton<IAppNameProvider>(sp =>
        {
            var settings = sp.GetService<IMessagePublishSettings>();
            var queueSettings = sp.GetService<IQueueSettings>();
            var appName = settings?.AppId ?? queueSettings?.AppId ?? appId;
            return new AppNameProvider(appName ?? Assembly.GetExecutingAssembly().GetName().Name);
        });
        
        services.TryAddSingleton<IRabbitMqConnectionSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IMessagePublishSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IPublisherChannelSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<IQueueSettings>(sc => MergeSettings(sc, options));
        services.TryAddSingleton<ITraceKeySettings>(sc => MergeSettings(sc, options));
        
        services.TryAddScoped<IHttpTraceHeadersProvider,HttpTraceHeadersProvider>();
        services.TryAddScoped<SubscriberTraceContextProvider>();
        services.TryAddScoped<ISubscriberTraceContextProvider>(sp => sp.GetRequiredService<SubscriberTraceContextProvider>());
        services.TryAddScoped<ITraceContextWriter>(sp => sp.GetRequiredService<SubscriberTraceContextProvider>());

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
        services.TryAddSingleton<ISetupRabbitMq, MessageListener>();
        services.TryAddSingleton<MessageSubscriber>();
        services.TryAddTransient<NotifyConsumeStateWrapper>();
        
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
    public string ConfigSectionName { get; set; } = "RelayPulse:RabbitMQ";
    public RabbitMqSettings? Settings { get; set; }
}