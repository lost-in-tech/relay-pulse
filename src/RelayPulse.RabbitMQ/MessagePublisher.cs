using System.Text;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal sealed class MessagePublisher(
    IRabbitMqWrapper rabbitMqWrapper,
    IMessagePublishSettings settings,
    IEnumerable<IChannelFactory> channelFactories,
    IMessageSerializer serializer,
    BasicPropertiesBuilder basicPropertiesBuilder,
    IEnumerable<IMessageFilter> filters)
    : IMessagePublisher
{
    public Task<bool> Publish<T>(Message<T> msg, CancellationToken ct)
    {
        foreach (var filter in filters)
        {
            msg = filter.Apply(msg);
        }
        
        var type = typeof(T);
        var typeFullName = msg.Type.EmptyAlternative(type.FullName.EmptyAlternative(type.Name));
        var typeName = $"{settings.TypePrefix}{msg.Type.EmptyAlternative(type.Name.ToSnakeCase())}";
        var typePath = new[] { $"{msg.AppId.EmptyAlternative("app")}", msg.Tenant, typeName}.Join("/");

        
        var tenant = msg.Tenant.TryPickNonEmpty(settings.DefaultTenant);
        if (tenant.HasValue())
        {
            msg.Headers[Constants.HeaderTenant] = tenant;
        }
        
        var appName = msg.AppId.TryPickNonEmpty(settings.AppId);

        if (appName.HasValue())
        {
            msg.Headers[Constants.HeaderAppId] = appName;
        }

        msg.Headers[settings.MessageTypeShortHeaderName.EmptyAlternative(Constants.HeaderMsgTypeShort)] = typeName;
        
        msg.Headers[settings.MessageTypeFullHeaderName.EmptyAlternative(Constants.HeaderMsgTypeFull)] = typePath;

        var exchange = msg.Headers.PopValue(Constants.HeaderExchange).EmptyAlternative(settings.DefaultExchange);

        if (string.IsNullOrWhiteSpace(exchange))
        {
            throw new Exception("Exchange name cannot be empty. Make sure you provide exchange name.");
        }

        var channelFactory = channelFactories.FirstOrDefault(x => x.IsApplicable(typeFullName, forPublisher: true));

        if (channelFactory == null)
        {
            throw new Exception($"No applicable channel factory available for publisher and {typeFullName}");
        }
        
        var channel = channelFactory.GetOrCreate(type.AssemblyQualifiedName.EmptyAlternative(typeFullName));

        var expiry = msg.Headers.PopAsDouble(Constants.HeaderExpiryKey);
        
        rabbitMqWrapper.BasicPublish(channel, new BasicPublishInput
        {
            Body = Encoding.UTF8.GetBytes(serializer.Serialize(msg.Content)),
            Exchange = exchange,
            RoutingKey = msg.Headers.PopValue(Constants.HeaderRoutingKey) ?? string.Empty,
            BasicProperties = basicPropertiesBuilder.Build(channel, 
                typeFullName, 
                msg,
                expiry ?? settings.DefaultExpiryInSeconds,
                appName)
        });

        return Task.FromResult(true);
    }

    public Task<bool> Publish<T>(T content, CancellationToken ct)
    {
        return Publish(new Message<T>
        {
            Content = content
        }, ct);
    }
}

internal interface IMessagePublishSettings
{
    public string? DefaultTenant { get; }
    public double? DefaultExpiryInSeconds { get; }
    public string? AppId { get; }
    public string DefaultExchange { get; }
    public string? TypePrefix { get; }
    public string? MessageTypeFullHeaderName { get; }
    public string? MessageTypeShortHeaderName { get; }
}