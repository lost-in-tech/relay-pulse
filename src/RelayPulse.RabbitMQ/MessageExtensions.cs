using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class MessageExtensions
{
    public static Message<T> Exchange<T>(this Message<T> source, string exchange)
    {
        source.Headers[Constants.HeaderExchange] = exchange;
        return source;
    }
    
    public static Message<T> RouteKey<T>(this Message<T> source, string routeKey)
    {
        source.Headers[Constants.HeaderRoutingKey] = routeKey;
        return source;
    }

    public static Message<T> Expiry<T>(this Message<T> source, int expiryInSeconds)
    {
        source.Headers[Constants.HeaderExpiryKey] = expiryInSeconds.ToString("F0");
        return source;
    }
}