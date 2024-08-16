using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class MessageExtensions
{
    public static void SetExchange<T>(this Message<T> msg, string exchange)
    {
        msg.Headers[Constants.HeaderExchange] = exchange;
    }
    
    public static string? GetExchange<T>(this Message<T> msg)
    {
        return msg.Headers.GetValueOrDefault(Constants.HeaderExchange);
    }
    
    public static void SetRouting<T>(this Message<T> msg, string routing)
    {
        msg.Headers[Constants.HeaderRoutingKey] = routing;
    }
    
    public static string? GetRouting<T>(this Message<T> msg)
    {
        return msg.Headers.GetValueOrDefault(Constants.HeaderRoutingKey);
    }
}