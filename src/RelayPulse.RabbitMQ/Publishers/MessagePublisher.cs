using System.Text;
using RabbitMQ.Client;
using RelayPulse.Core;
using RelayPulse.RabbitMQ.Publishers;

namespace RelayPulse.RabbitMQ;

internal sealed class 
    MessagePublisher(
    IRabbitMqWrapper rabbitMqWrapper,
    IMessagePublishSettings settings,
    IEnumerable<IChannelFactory> channelFactories,
    IMessageSerializer serializer,
    IUniqueId uniqueId,
    BasicPropertiesBuilder basicPropertiesBuilder,  
    IAppNameProvider appNameProvider,
    IEnumerable<IMessageFilter> filters)
    : IMessagePublisher
{
    public Task<MessagePublishResponse> Publish<T>(Message<T> msg, CancellationToken ct)
    {
        foreach (var filter in filters)
        {
            msg = filter.Apply(msg);
        }

        var type = typeof(T);

        var typeName = msg.Type.EmptyAlternative(type.FullName.EmptyAlternative(type.Name));
        
        var exchange = msg.Headers.PopValue(Constants.HeaderExchange).EmptyAlternative(settings.DefaultExchange);

        if (string.IsNullOrWhiteSpace(exchange))
        {
            throw new RelayPulseException("Exchange name cannot be empty. Make sure you provide exchange name.");
        }

        var channel = GetChannel(typeName, type);

        var id = msg.Id ?? uniqueId.New();
        var props = basicPropertiesBuilder.Build(id, channel, msg);

        if (string.IsNullOrWhiteSpace(props.AppId))
        {
            props.AppId = appNameProvider.Get();
        }
        
        rabbitMqWrapper.BasicPublish(channel, new BasicPublishInput
        {
            Body = Encoding.UTF8.GetBytes(serializer.Serialize(msg.Content)),
            Exchange = exchange,
            RoutingKey = msg.Headers.PopValue(Constants.HeaderRoutingKey) ?? string.Empty,
            BasicProperties = props,
        });

        return Task.FromResult(new MessagePublishResponse
        {
            Id = id
        });
    }

    public Task<MessagePublishResponse> Publish<T>(T content, CancellationToken ct)
    {
        return Publish(new Message<T>
        {
            Content = content
        }, ct);
    }

    private IModel GetChannel(string typeName, Type type)
    {
        var channelFactory = channelFactories.FirstOrDefault(x => x.IsApplicable(typeName, forPublisher: true));

        if (channelFactory == null)
        {
            throw new Exception($"No applicable channel factory available for publisher and {typeName}");
        }
        
        var channel = channelFactory.GetOrCreate(type.AssemblyQualifiedName.EmptyAlternative(typeName));
        return channel;
    }
}

internal interface IMessagePublishSettings
{
    public string? DefaultTenant { get; }
    public double? DefaultExpiryInSeconds { get; }
    public string? AppId { get; }
    public string DefaultExchange { get; }
    public string? TypePrefix { get; }
    public string? MessageTypeHeaderName { get; }
    public string? SentAtHeaderName { get; }
    public string? TenantHeaderName { get; }
    public string? AppIdHeaderName { get; }
}