using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public interface IHaveMessageContent : IPublishMessage, ICollectExchangeName, ICollectRoutingKey, ICollectMsgHeaders
{
    
}

public interface ICollectRoutingKey
{
    IHaveRoutingKey Routing(string routingKey);
}

public interface IHaveRoutingKey : ICollectMsgHeaders, IPublishMessage
{
    
}

public interface ICollectExchangeName
{
    IHaveExchangeName Exchange(string name);
}

public interface IHaveExchangeName : ICollectRoutingKey, ICollectMsgHeaders, IPublishMessage
{   
}

public interface ICollectMsgHeaders
{
    ICollectMsgHeaders Header(string name, string? value);
    ICollectMsgHeaders Headers(Dictionary<string,string?> value);
}

public interface IHaveMsgHeaders : ICollectMsgHeaders, IPublishMessage
{
    
}

public interface IPublishMessage
{
    Task<bool> Publish(CancellationToken ct = default);
}

internal sealed class FluentMessagePublisher<T>(IMessagePublisher publisher, Message<T> msg)
    : IHaveMessageContent, IHaveExchangeName, IHaveRoutingKey
{
    public Task<bool> Publish(CancellationToken ct)
    {
        return publisher.Publish(msg, ct);
    }

    public IHaveExchangeName Exchange(string name)
    {
        msg.Headers[Constants.HeaderExchange] = name;
        return this;
    }

    public IHaveRoutingKey Routing(string routingKey)
    {
        msg.Headers[Constants.HeaderRoutingKey] = routingKey;
        return this;
    }

    public ICollectMsgHeaders Header(string name, string? value)
    {
        if (value == null) return this;
        msg.Headers[name] = value;
        return this;
    }

    public ICollectMsgHeaders Headers(Dictionary<string, string?> value)
    {
        if (value.Count == 0) return this;

        foreach (var kv in value)
        {
            if(kv.Value == null) continue;

            msg.Headers[kv.Key] = kv.Value;
        }

        return this;
    }
}


public static class MessagePublisherExtensions
{
    public static IHaveMessageContent Message<T>(this IMessagePublisher publisher, T content)
        => new FluentMessagePublisher<T>(publisher, new Message<T>
        {
            Content = content
        });
    
    public static IHaveMessageContent Message<T>(this IMessagePublisher publisher, Message<T> msg)
        => new FluentMessagePublisher<T>(publisher, msg);
}