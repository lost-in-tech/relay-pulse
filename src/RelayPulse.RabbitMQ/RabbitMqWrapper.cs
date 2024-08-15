using System.Text;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IRabbitMqWrapper
{
    void BasicPublish<T>(IModel channel, BasicPublishInput<T> input);
}

internal sealed class RabbitMqWrapper(IMessageSerializer serializer) 
    : IRabbitMqWrapper
{
    public void BasicPublish<T>(IModel channel, BasicPublishInput<T> input)
    {
        var prop = BasicPropertiesBuilder.Build(channel, input);
        var body = serializer.Serialize(input.Body);
        
        channel.BasicPublish(exchange: input.Exchange, 
            routingKey: input.RoutingKey, 
            basicProperties: prop, 
            body: Encoding.UTF8.GetBytes(body),
            mandatory: false);
    }
}



internal static class BasicPropertiesBuilder
{
    public static IBasicProperties Build<T>(IModel channel, BasicPublishInput<T> input)
    {
        var prop = channel.CreateBasicProperties();
        
        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            prop.Type = input.Type;
        }

        if (!string.IsNullOrWhiteSpace(input.AppId))
        {
            prop.AppId = input.AppId;
        }
        
        if (!string.IsNullOrWhiteSpace(input.UserId))
        {
            prop.UserId = input.UserId;
        }

        prop.ContentEncoding = "utf-8";

        if (!string.IsNullOrWhiteSpace(prop.CorrelationId))
        {
            prop.CorrelationId = input.Cid;
        }

        if (!string.IsNullOrWhiteSpace(input.MsgId))
        {
            prop.MessageId = input.MsgId;
        }

        if (input.Headers != null)
        {
            prop.Headers ??= new Dictionary<string, object>();

            foreach (var header in input.Headers)
            {
                prop.Headers[header.Key] = header.Value;
            }
        }

        return prop;
    }
}

public record BasicPublishInput<T>
{
    public required string Exchange { get; init; }
    public required string RoutingKey { get; init; }
    public required T Body { get; init; }
    
    public Dictionary<string,object>? Headers { get; init; }
    public string? Type { get; init; }
    public string? AppId { get; init; }
    public string? MsgId { get; init; }
    public string? Cid { get; init; }
    public string? UserId { get; init; }
}