using RelayPulse.RabbitMQ.Subscribers;

namespace RelayPulse.RabbitMQ;

public record RabbitMqSettings : 
    IRabbitMqConnectionSettings, 
    IMessagePublishSettings,
    IPublisherChannelSettings,
    IQueueSettings
{
    public string? AppId { get; set; }
    public string Uri { get; set; } = string.Empty;
    public string DefaultExchange { get; set; } = string.Empty;
    
    /// <summary>
    /// Valid values are null, fanout, direct, topic and headers
    /// </summary>
    public string? DefaultExchangeType { get; set; }

    public int? DefaultPrefetchCount { get; set; }

    public string? DefaultTenant { get; set; }
    
    /// <summary>
    /// Type prefix to use for type name that system gonna pass in header 
    /// </summary>
    public string? TypePrefix { get; set; }
    
    public double? DefaultExpiryInSeconds { get; set; }
    
    public string? MessageTypeHeaderName { get; set; }
    
    public string? TenantHeaderName { get; set; }
    
    public string? AppIdHeaderName { get; set; }

    /// <summary>
    /// Optional and default is false. When set to true for each message type a new channel will be used
    /// </summary>
    public bool? UseChannelPerType { get; set; }

    public QueueSettings[]? Queues { get; set; }
    
    public string? SentAtHeaderName { get; set; }
}

public record QueueSettings
{
    public bool? SkipSetup { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// If empty default exchange name will be used
    /// </summary>
    public string? Exchange { get; set; }
    /// <summary>
    /// Valid values are null, fanout, direct, topic and headers
    /// </summary>
    public string? ExchangeType { get; set; }

    public int? MsgExpiryInSeconds { get; set; }

    public bool DeadLetterDisabled { get; set; }
    /// <summary>
    /// Default will be "{exchange-name}-dlx if not disabled";
    /// </summary>
    public string? DeadLetterExchange { get; set; }
    /// <summary>
    /// Default will be "{exchange-name}-dlq if not disabled";
    /// </summary>
    public string? DeadLetterQueue { get; set; }

    public bool RetryFeatureDisabled { get; set; }
    /// <summary>
    /// Default will be "{exchange-name}-rtx if not disabled";
    /// </summary>
    public string? RetryExchange { get; set; }
    /// <summary>
    /// Default will be "{exchange-name}-rtq if not disabled";
    /// </summary>
    public string? RetryQueue { get; set; }
    /// <summary>
    /// Default will be 60 seconds
    /// </summary>
    public int? RetryDelayInSeconds { get; set; }
    public QueueBinding[]? Bindings { get; set; }
    
    public int? PrefetchCount { get; set; }
}

public record QueueBinding
{
    public bool? MatchAny { get; set; }
    public string? RoutingKey { get; set; }
    public Dictionary<string,string>? Headers { get; set; }
}