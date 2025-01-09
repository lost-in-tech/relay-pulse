using System.Text;
using System.Text.Unicode;
using NSubstitute;
using RabbitMQ.Client;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqWrapper(IMessageSerializer serializer) : IRabbitMqWrapper
{
    private BasicPublishInput? _lastPublishInput;
    private int _totalPublishExecutionCount;
    private readonly List<ExchangeDeclareCallInfo> _exchangeDeclareCalls = new();
    private readonly List<QueueDeclareCallInfo> _queueDeclareCalls = new();
    private readonly List<QueueBindCallInfo> _queueBindCalls = new();


    public ExchangeDeclareCallInfo[] ExchangeDeclareCalls => _exchangeDeclareCalls.ToArray();
    public QueueDeclareCallInfo[] QueueDeclareCalls => _queueDeclareCalls.ToArray();
    public QueueBindCallInfo[] QueueBindCalls => _queueBindCalls.ToArray();
    
    
    public RabbitMqPublishCallInfo<T> GetLastUsedPublishInput<T>()
    {
        if (_lastPublishInput == null) return new RabbitMqPublishCallInfo<T>();

        using var ms = new MemoryStream(_lastPublishInput.Body.ToArray());
        
        return new RabbitMqPublishCallInfo<T>
        {
            Body = serializer.Deserialize<T>(ms),
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

    public void ExchangeDeclare(IModel channel, string name, string type)
    {
        _exchangeDeclareCalls.Add(new ExchangeDeclareCallInfo
        {
            Name = name,
            Type = type
        });
    }

    public void QueueDeclare(IModel channel, string queue, Dictionary<string, object>? args, int? prefetchCount)
    {
        _queueDeclareCalls.Add(new QueueDeclareCallInfo
        {
            Name = queue,
            Args = args,
            PrefetchCount = prefetchCount
        });
    }

    public void QueueBind(IModel channel, string queue, string exchange, string routingKey, Dictionary<string, object>? args)
    {
        _queueBindCalls.Add(new QueueBindCallInfo
        {
            Exchange = exchange,
            Queue = queue,
            RouteKey = routingKey,
            Args = args
        });
    }

    public static FakeRabbitMqWrapper New() => new(new MessageSerializer());
}

public class RabbitMqPublishCallInfo<T>
{
    public int ExecutionCount { get; init; }
    public BasicPublishInput? LastInput { get; init; }
    public T? Body { get; init; }
}

public class ExchangeDeclareCallInfo
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public class QueueDeclareCallInfo
{
    public required string Name { get; init; }
    public required Dictionary<string,object>? Args { get; init; }
    public int? PrefetchCount { get; init; }
}

public class QueueBindCallInfo
{
    public required string Queue { get; init; }
    public required string Exchange { get; init; }
    public required string RouteKey { get; init; }
    public Dictionary<string,object>? Args { get; init; }
}