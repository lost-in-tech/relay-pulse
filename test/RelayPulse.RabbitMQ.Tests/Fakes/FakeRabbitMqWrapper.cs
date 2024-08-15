using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqWrapper : IRabbitMqWrapper
{
    public BasicPublishInput<object>? LastInput { get; private set; }
    public int TotalExecutionCount { get; private set; }
    
    public void BasicPublish<T>(IModel channel, BasicPublishInput<T> input)
    {
        LastInput = new BasicPublishInput<object>
        {
            Body = input.Body!,
            Exchange = input.Exchange,
            RoutingKey = input.RoutingKey,
            Cid = input.Cid,
            Type = input.Type,
            AppId = input.AppId,
            MsgId = input.MsgId,
            Headers = input.Headers,
            UserId = input.UserId
        };
        
        TotalExecutionCount++;
    }
}