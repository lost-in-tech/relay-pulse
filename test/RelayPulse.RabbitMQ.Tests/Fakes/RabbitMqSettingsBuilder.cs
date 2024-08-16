namespace RelayPulse.RabbitMQ.Tests.Fakes;

public static class RabbitMqSettingsBuilder
{
    public static RabbitMqSettings Build()
    {
        return new RabbitMqSettings
        {
            Uri = "amqps://guest:guest@localhost/rabbit",
            DefaultExchange = "bookworm.events"
        };
    }
}