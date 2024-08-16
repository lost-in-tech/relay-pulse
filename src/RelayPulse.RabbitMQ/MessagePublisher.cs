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
        var typeName = string.IsNullOrWhiteSpace(settings.TypePrefix) 
                        ? fullTypeName
                        : $"{settings.TypePrefix}{type.Name}";

        foreach (var filter in filters)
        {
            msg = filter.Apply(msg);
        }

        msg.Headers[settings.MessageTypeHeaderName ?? Constants.HeaderMsgType] = typeName;

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
        
        rabbitMqWrapper.BasicPublish(channel, new BasicPublishInput
        {
            Body = Encoding.UTF8.GetBytes(serializer.Serialize(msg.Content)),
            Exchange = exchange,
            RoutingKey = PopValue(msg.Headers, Constants.HeaderRoutingKey) ?? string.Empty,
            BasicProperties = basicPropertiesBuilder.Build(channel, typeName, msg)
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

    private Dictionary<string, object>? BuildHeaders(Dictionary<string, string>? source)
    {
        if (source == null) return null;

        var result = new Dictionary<string, object>();

        foreach (var kv in source)
        {
            result[kv.Key] = kv.Value;
        }

        return result;
    }

    private string? PopValue(Dictionary<string, string>? headers, string key)
    {
        if (headers == null) return null;
        var result = headers.GetValueOrDefault(key);
        if (result != null) headers.Remove(key);
        return result;
    }
}

internal interface IMessagePublishSettings
{
    public string DefaultExchange { get; }
    public string? TypePrefix { get; }
    public string? MessageTypeHeaderName { get; }
}