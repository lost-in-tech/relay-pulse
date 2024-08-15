using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IRabbitMqConnectionInstance
{
    IConnection Get();
}

internal interface IRabbitMqConnectionSettings
{
    public string Uri { get; init; }
}

internal sealed class RabbitMqConnectionInstance : IRabbitMqConnectionInstance
{
    private readonly Lazy<IConnection> _instance;
    
    public RabbitMqConnectionInstance(IRabbitMqConnectionSettings settings)
    {
        _instance = new Lazy<IConnection>(() =>
        {
            var factory = new ConnectionFactory
            {
                DispatchConsumersAsync = true,
                Uri = new Uri(settings.Uri)
            };

            return factory.CreateConnection();
        });
    }

    public IConnection Get() => _instance.Value;
}