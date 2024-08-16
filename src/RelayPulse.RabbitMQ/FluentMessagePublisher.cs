using RelayPulse.Core.Fluent;

namespace RelayPulse.RabbitMQ;

public static class HaveMessageExtensions
{
    public static IHaveHeaders Exchange(this IHaveMessage msg, string exchange)
    {
        return msg.Header(Constants.HeaderExchange, exchange);
    }
    
    public static IHaveHeaders Exchange(this IHaveTenant msg, string exchange)
    {
        return msg.Header(Constants.HeaderExchange, exchange);
    }
    
    public static IHaveHeaders Exchange(this IHaveHeaders msg, string exchange)
    {
        return msg.Header(Constants.HeaderExchange, exchange);
    }
    
    public static IHaveHeaders Routing(this IHaveMessage msg, string routingKey)
    {
        return msg.Header(Constants.HeaderRoutingKey, routingKey);
    }
    
    public static IHaveHeaders Routing(this IHaveTenant msg, string routingKey)
    {
        return msg.Header(Constants.HeaderRoutingKey, routingKey);
    }
    
    public static IHaveHeaders Routing(this IHaveHeaders msg, string routingKey)
    {
        return msg.Header(Constants.HeaderRoutingKey, routingKey);
    }
    
    
    
    public static IHaveHeaders Expiry(this IHaveMessage msg, double seconds) 
        => msg.Header(Constants.HeaderExpiryKey, $"{seconds}");
    
    public static IHaveHeaders Expiry(this IHaveTenant msg, double seconds) 
        => msg.Header(Constants.HeaderExpiryKey, $"{seconds}");

    public static IHaveHeaders Expiry(this IHaveHeaders msg, double seconds) 
        => msg.Header(Constants.HeaderExpiryKey, $"{seconds}");

    public static IHaveHeaders Expiry(this IHaveMessage msg, TimeSpan expiry) 
        => msg.Expiry(expiry.TotalSeconds);
    
    public static IHaveHeaders Expiry(this IHaveTenant msg, TimeSpan expiry) 
        => msg.Expiry(expiry.TotalSeconds);

    public static IHaveHeaders Expiry(this IHaveHeaders msg, TimeSpan expiry) 
        => msg.Expiry(expiry.TotalSeconds);
}