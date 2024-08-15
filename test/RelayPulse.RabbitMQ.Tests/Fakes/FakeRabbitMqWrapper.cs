using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqWrapper : IRabbitMqWrapper
{
    private BasicPublishInput<object>? _lastPublishInput;
    private int _totalPublishExecutionCount;

    public BasicPublishInput<T> GetLastUsedPublishInput<T>() => new()
    {
        Body = (T)(_lastPublishInput!.Body),
        Exchange = _lastPublishInput.Exchange,
        RoutingKey = _lastPublishInput.RoutingKey,
        Cid = _lastPublishInput.Cid,
        Type = _lastPublishInput.Type,
        AppId = _lastPublishInput.AppId,
        MsgId = _lastPublishInput.MsgId,
        Headers = _lastPublishInput.Headers,
        UserId = _lastPublishInput.UserId
    };

    public int GetPublishExecutionCount() => _totalPublishExecutionCount;
    
    public void BasicPublish<T>(IModel channel, BasicPublishInput<T> input)
    {
        _lastPublishInput = new BasicPublishInput<object>
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
        
        _totalPublishExecutionCount++;
    }
}