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



// internal static class BasicPropertiesBuilder
// {
//     public static IBasicProperties Build<T>(IModel channel, BasicPublishInput<T> input)
//     {
//         var prop = channel.CreateBasicProperties();
//         
//         if (!string.IsNullOrWhiteSpace(input.Type))
//         {
//             prop.Type = input.Type;
//         }
//
//         if (!string.IsNullOrWhiteSpace(input.AppId))
//         {
//             prop.AppId = input.AppId;
//         }
//         
//         if (!string.IsNullOrWhiteSpace(input.UserId))
//         {
//             prop.UserId = input.UserId;
//         }
//
//         prop.ContentEncoding = "utf-8";
//
//         if (!string.IsNullOrWhiteSpace(prop.CorrelationId))
//         {
//             prop.CorrelationId = input.Cid;
//         }
//
//         if (!string.IsNullOrWhiteSpace(input.MsgId))
//         {
//             prop.MessageId = input.MsgId;
//         }
//
//         if (input.Headers != null)
//         {
//             prop.Headers ??= new Dictionary<string, object>();
//
//             foreach (var header in input.Headers)
//             {
//                 prop.Headers[header.Key] = header.Value;
//             }
//         }
//
//         return prop;
//     }
// }

public record BasicPublishInput
{
    public required string Exchange { get; init; }
    public required string RoutingKey { get; init; }
    
    public required ReadOnlyMemory<byte> Body { get; init; }
    
    public required IBasicProperties BasicProperties { get; init; }
}