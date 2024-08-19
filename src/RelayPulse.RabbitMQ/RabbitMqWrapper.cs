using System.Text;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IRabbitMqWrapper
{
    void BasicPublish(IModel channel, BasicPublishInput input);
    void ExchangeDeclare(IModel channel, string name, string type);
    void QueueDeclare(IModel channel, string queue, Dictionary<string, object>? args);
    void QueueBind(IModel channel, string queue, string exchange, string routingKey, Dictionary<string, object>? args);
}

internal sealed class RabbitMqWrapper 
    : IRabbitMqWrapper
{
    public void BasicPublish(IModel channel, BasicPublishInput input)
    {
        channel.BasicPublish(exchange: input.Exchange, 
            routingKey: input.RoutingKey, 
            basicProperties: input.BasicProperties, 
            body: input.Body,
            mandatory: false);
    }

    public void ExchangeDeclare(IModel channel, string name, string type)
    {
        channel.ExchangeDeclare(name, type, true);
    }

    public void QueueDeclare(IModel channel, string queue, Dictionary<string, object>? args)
    {
        channel.QueueDeclare(queue, true, false, false, args);
    }

    public void QueueBind(IModel channel, string queue, string exchange, string routingKey, Dictionary<string, object>? args)
    {
        channel.QueueBind(queue, exchange, routingKey, args);
    }
}

public record BasicPublishInput
{
    public required string Exchange { get; init; }
    public required string RoutingKey { get; init; }
    
    public required ReadOnlyMemory<byte> Body { get; init; }
    
    public required IBasicProperties BasicProperties { get; init; }
}