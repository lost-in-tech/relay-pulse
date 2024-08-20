using RelayPulse.RabbitMQ.Subscribers;

namespace RelayPulse.RabbitMQ;

public record RabbitMqSettings : 
    IRabbitMqConnectionSettings, 
    IMessagePublishSettings,
    IPublisherChannelSettings,
    IQueueSettings
{
    public string? AppId { get; init; }
    public string Uri { get; init; } = string.Empty;
    public string DefaultExchange { get; init; } = string.Empty;
    
    /// <summary>
    /// Valid values are null, fanout, direct, topic and headers
    /// </summary>
    public string? DefaultExchangeType { get; init; }

    public int? DefaultPrefetchCount { get; init; }

    public string? DefaultTenant { get; init; }
    
    /// <summary>
    /// Type prefix to use for type name that system gonna pass in header 
    /// </summary>
    public string? TypePrefix { get; init; }
    
    public double? DefaultExpiryInSeconds { get; init; }
    
    public string? MessageTypeHeaderName { get; init; }
    
    public string? TenantHeaderName { get; init; }
    
    public string? AppIdHeaderName { get; init; }

    /// <summary>
    /// Optional and default is false. When set to true for each message type a new channel will be used
    /// </summary>
    public bool? UseChannelPerType { get; init; }

    public QueueSettings[]? Queues { get; init; }
    
    public string? SentAtHeaderName { get; init; }
}

public record QueueSettings
{
    public bool? SkipSetup { get; init; }
    public required string Name { get; init; }
    /// <summary>
    /// If empty default exchange name will be used
    /// </summary>
    public string? Exchange { get; init; }
    /// <summary>
    /// Valid values are null, fanout, direct, topic and headers
    /// </summary>
    public string? ExchangeType { get; init; }

    public int? MsgExpiryInSeconds { get; init; }

    public bool DeadLetterDisabled { get; init; }
    /// <summary>
    /// Default will be "{exchange-name}-dlx if not disabled";
    /// </summary>
    public string? DeadLetterExchange { get; init; }
    /// <summary>
    /// Default will be "{exchange-name}-dlq if not disabled";
    /// </summary>
    public string? DeadLetterQueue { get; init; }

    public bool RetryFeatureDisabled { get; init; }
    /// <summary>
    /// Default will be "{exchange-name}-rtx if not disabled";
    /// </summary>
    public string? RetryExchange { get; init; }
    /// <summary>
    /// Default will be "{exchange-name}-rtq if not disabled";
    /// </summary>
    public string? RetryQueue { get; init; }
    /// <summary>
    /// Default will be 60 seconds
    /// </summary>
    public int? RetryDelayInSeconds { get; init; }
    public QueueBinding? Binding { get; init; }
    
    public int? PrefetchCount { get; init; }
}

public record QueueBinding
{
    public string[]? RoutingKeyBindings { get; init; }
    public IEnumerable<HeaderBinding>? HeaderBindings { get; init; }
}

public record HeaderBinding
{
    public bool MatchAny { get; init; }
    public required Dictionary<string,string> Args { get; init; }
}