using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IRabbitMqConnectionInstance
{
    IConnection Get();
}

internal interface IRabbitMqConnectionSettings
{
    public string Uri { get; }
}

internal sealed class RabbitMqConnectionInstance : IRabbitMqConnectionInstance
{
    private readonly Lazy<IConnection> _instance;
    
    public RabbitMqConnectionInstance(IRabbitMqConnectionSettings settings)
    {
        _instance = new Lazy<IConnection>(() =>
        {
            if (string.IsNullOrWhiteSpace(settings.Uri))
                throw new Exception("No connection string provided for rabbitmq");
            
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