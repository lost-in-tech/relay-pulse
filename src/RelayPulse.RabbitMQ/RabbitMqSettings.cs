namespace RelayPulse.RabbitMQ;

public record RabbitMqSettings : IRabbitMqConnectionSettings, IMessagePublishSettings
{
    public required string Uri { get; init; }
    public string DefaultExchange { get; init; } = string.Empty;
}