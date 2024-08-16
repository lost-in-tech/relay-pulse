namespace RelayPulse.RabbitMQ;

public record RabbitMqSettings : 
    IRabbitMqConnectionSettings, 
    IMessagePublishSettings,
    IPublisherChannelSettings
{
    public string Uri { get; init; } = string.Empty;
    public string DefaultExchange { get; init; } = string.Empty;
    
    /// <summary>
    /// Type prefix to use for type name that system gonna pass in header 
    /// </summary>
    public string? TypePrefix { get; init; }

    /// <summary>
    /// name of header used to provide message type. default value is "r-msg-type"
    /// </summary>
    public string? MessageTypeHeaderName { get; init; }
    
    /// <summary>
    /// Optional and default is false. When set to true for each message type a new channel will be used
    /// </summary>
    public bool? UseChannelPerType { get; init; }
}