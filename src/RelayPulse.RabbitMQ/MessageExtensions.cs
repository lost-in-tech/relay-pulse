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
    
    public static void SetExpiry<T>(this Message<T> msg, double seconds)
    {
        msg.Headers[Constants.HeaderExpiryKey] = $"{seconds}";
    }

    public static void SetExpiry<T>(this Message<T> msg, TimeSpan expiry) =>
        SetExpiry(msg, expiry.TotalSeconds);
    
    public static double? GetExpiryInSeconds<T>(this Message<T> msg)
    {
        var result = msg.Headers.GetValueOrDefault(Constants.HeaderExpiryKey);

        if (result.HasValue()) return double.TryParse(result, out var expiry) ? expiry : null;

        return null;
    }
}