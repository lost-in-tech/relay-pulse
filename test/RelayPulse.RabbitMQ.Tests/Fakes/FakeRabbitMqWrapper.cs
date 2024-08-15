using System.Text;
using System.Text.Unicode;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqWrapper(IMessageSerializer serializer) : IRabbitMqWrapper
{
    private BasicPublishInput? _lastPublishInput;
    private int _totalPublishExecutionCount;

    public RabbitMqPublishCallInfo<T> GetLastUsedPublishInput<T>()
    {
        if (_lastPublishInput == null) return new RabbitMqPublishCallInfo<T>();
        
        return new RabbitMqPublishCallInfo<T>
        {
            Body = serializer.Deserialize<T>(Encoding.UTF8.GetString(_lastPublishInput.Body.ToArray())),
            ExecutionCount = _totalPublishExecutionCount,
            LastInput = _lastPublishInput
        };
    }

    public int GetPublishExecutionCount() => _totalPublishExecutionCount;
    
    public void BasicPublish(IModel channel, BasicPublishInput input)
    {
        _lastPublishInput = input;
        
        _totalPublishExecutionCount++;
    }
}

public class RabbitMqPublishCallInfo<T>
{
    public int ExecutionCount { get; init; }
    public BasicPublishInput? LastInput { get; init; }
    public T? Body { get; init; }
}