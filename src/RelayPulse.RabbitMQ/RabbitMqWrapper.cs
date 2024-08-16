using System.Text;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IRabbitMqWrapper
{
    void BasicPublish(IModel channel, BasicPublishInput input);
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
}

public record BasicPublishInput
{
    public required string Exchange { get; init; }
    public required string RoutingKey { get; init; }
    
    public required ReadOnlyMemory<byte> Body { get; init; }
    
    public required IBasicProperties BasicProperties { get; init; }
}