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
        var type = typeof(T);
        var fullTypeName = type.FullName ?? type.Name;

        foreach (var filter in filters)
        {
            msg = filter.Apply(msg);
        }
        
        var appName = msg.AppId.TryPickNonEmpty(settings.AppId);
        
        var typeName = $"{appName.EmptyAlternative("app")}/{settings.TypePrefix}{type.Name}";
        
        msg.Headers[settings.MessageTypeHeaderName.EmptyAlternative(Constants.HeaderMsgType)] = typeName.ToSnakeCase();

        var exchange = PopValue(msg.Headers, Constants.HeaderExchange) ?? settings.DefaultExchange;

        if (string.IsNullOrWhiteSpace(exchange))
        {
            throw new Exception("Exchange name cannot be empty. Make sure you provide exchange name.");
        }

        var channelFactory = channelFactories.FirstOrDefault(x => x.IsApplicable(fullTypeName, forPublisher: true));

        if (channelFactory == null)
        {
            throw new Exception($"No applicable channel factory available for publisher and {fullTypeName}");
        }
        
        var channel = channelFactory.GetOrCreate(typeName);

        var expiry = PopValueAsDouble(msg.Headers, Constants.HeaderExpiryKey);
        
        rabbitMqWrapper.BasicPublish(channel, new BasicPublishInput
        {
            Body = Encoding.UTF8.GetBytes(serializer.Serialize(msg.Content)),
            Exchange = exchange,
            RoutingKey = PopValue(msg.Headers, Constants.HeaderRoutingKey) ?? string.Empty,
            BasicProperties = basicPropertiesBuilder.Build(channel, 
                fullTypeName, 
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

    private string? PopValue(Dictionary<string, string>? headers, string key)
    {
        if (headers == null) return null;
        var result = headers.GetValueOrDefault(key);
        if (result != null) headers.Remove(key);
        return result;
    }
    
    private double? PopValueAsDouble(Dictionary<string, string>? headers, string key)
    {
        var result = PopValue(headers, key);
        if (result.HasValue()) return double.TryParse(result, out var longValue) ? longValue : null;
        return null;
    }
}

internal interface IMessagePublishSettings
{
    public double? DefaultExpiryInSeconds { get; }
    public string? AppId { get; }
    public string DefaultExchange { get; }
    public string? TypePrefix { get; }
    public string? MessageTypeHeaderName { get; }
}