using RelayPulse.Core.Fluent;

namespace RelayPulse.RabbitMQ;

public static class HaveMessageExtensions
{
    public static IHaveHeaders Exchange(this IHaveMessage msg, string exchange)
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
    
    public static IHaveHeaders Routing(this IHaveHeaders msg, string routingKey)
    {
        return msg.Header(Constants.HeaderRoutingKey, routingKey);
    }
}