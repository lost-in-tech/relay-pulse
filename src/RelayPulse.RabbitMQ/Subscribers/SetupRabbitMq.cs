namespace RelayPulse.RabbitMQ.Subscribers;

internal class SetupRabbitMq(
    IRabbitMqWrapper wrapper, 
    IRabbitMqConnectionInstance connectionInstance)
{
    public QueueInfo[] Run(IQueueSettings settings)
    {
        var result = new List<QueueInfo>();

        if (settings.Queues == null) return Array.Empty<QueueInfo>();

        var exchangeCreated = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var channel = connectionInstance.Get().CreateModel();
        
        foreach (var queue in settings.Queues)
        {
            var exchange = queue.Exchange.TryPickNonEmpty(settings.DefaultExchange) ?? string.Empty;

            if (!exchange.HasValue()) throw new Exception("Exchange name cannot be empty");
            
            var exchangeType = queue.ExchangeType.TryPickNonEmpty(settings.DefaultExchangeType)
                .EmptyAlternative(Constants.ExchangeTypeDirect);
            
            if (queue.SkipSetup ?? false)
            {
                result.Add(new QueueInfo
                {
                    Exchange = exchange,
                    Name = queue.Name,
                    ExchangeType = exchangeType,
                    RetryExchange = queue.RetryFeatureDisabled ? null : queue.RetryExchange.EmptyAlternative($"{exchange}-rtx"),
                    RetryQueue = queue.RetryFeatureDisabled ? null : queue.RetryQueue.EmptyAlternative($"{exchange}-rtq"),
                    DeadLetterExchange = queue.DeadLetterDisabled ? null : queue.DeadLetterExchange
                });
                
                continue;
            }

            if (!exchangeCreated.ContainsKey(exchange))
            {
                wrapper.ExchangeDeclare(channel, exchange, exchangeType);
                
                exchangeCreated[exchange] = exchange;
            }

            var queueDeclareArgs = new Dictionary<string, object>();

            if (queue.MsgExpiryInSeconds is > 0)
            {
                queueDeclareArgs[Constants.HeaderTimeToLive] = queue.MsgExpiryInSeconds.Value * 1000;
            }

            var deadLetterExchange = queue.DeadLetterExchange;
            if (!queue.DeadLetterDisabled)
            {
                deadLetterExchange = queue.DeadLetterExchange.EmptyAlternative($"{exchange}-dlx");

                if (!exchangeCreated.ContainsKey(deadLetterExchange))
                {
                    wrapper.ExchangeDeclare(channel, deadLetterExchange, Constants.ExchangeTypeDirect);
                
                    exchangeCreated[deadLetterExchange] = deadLetterExchange;
                }

                var deadLetterQueue = queue.DeadLetterQueue.EmptyAlternative($"{exchange}-dlq");
                wrapper.QueueDeclare(channel, deadLetterQueue, null);
                
                wrapper.QueueBind(channel, deadLetterQueue, deadLetterExchange, string.Empty, null);

                queueDeclareArgs[Constants.HeaderDeadLetterExchange] = deadLetterExchange;
            }

            var retryExchange = queue.RetryExchange;

            if (!queue.RetryFeatureDisabled)
            {
                retryExchange = queue.RetryExchange.EmptyAlternative($"{exchange}-rtx");

                if (!exchangeCreated.ContainsKey(retryExchange))
                {
                    wrapper.ExchangeDeclare(channel, retryExchange, Constants.ExchangeTypeTopic);
                
                    exchangeCreated[retryExchange] = retryExchange;
                }

                var retryQueue = queue.RetryQueue.EmptyAlternative($"{exchange}-rtq");

                var retryQueueArgs = new Dictionary<string, object>
                {
                    [Constants.HeaderDeadLetterExchange] = exchange
                };
                var retryDelay = queue.RetryDelayInSeconds ?? 60;
                retryQueueArgs[Constants.HeaderTimeToLive] = retryDelay * 1000;
                
                wrapper.QueueDeclare(channel, retryQueue, retryQueueArgs);
                
                wrapper.QueueBind(channel, retryQueue, retryExchange, "*", null);
            }
            
            wrapper.QueueDeclare(channel, queue.Name, queueDeclareArgs);

            var binding = queue.Binding ?? new QueueBinding();

            if (exchangeType == Constants.ExchangeTypeHeader)
            {
                if (!queue.RetryFeatureDisabled)
                {
                    wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, new Dictionary<string, object>
                    {
                        [Constants.HeaderMatch] = "all",
                        [Constants.HeaderTargetQueue] = queue.Name
                    });
                }

                if (binding.HeaderBindings != null)
                {
                    foreach (var headerBinding in binding.HeaderBindings)
                    {
                        var bindingArgs = new Dictionary<string, object>();

                        var givenArgs = headerBinding.Args;

                        foreach (var arg in givenArgs)
                        {
                            bindingArgs[arg.Key] = arg.Value;
                        }

                        if (headerBinding.MatchAny)
                        {
                            bindingArgs[Constants.HeaderMatch] = "any";
                        }
                        else
                        {
                            bindingArgs[Constants.HeaderMatch] = "all";
                        }
                        
                        wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, bindingArgs);
                    }
                }
            }
            else if (exchangeType != Constants.ExchangeTypeFanout)
            {
                if (!queue.RetryFeatureDisabled)
                {
                    wrapper.QueueBind(channel, queue.Name, exchange, Constants.RouteKeyTargetQueue(queue.Name), null);
                }

                if (binding.RoutingKeyBindings?.Length is > 0) 
                {
                    foreach (var routingKeyBinding in binding.RoutingKeyBindings)
                    {
                        wrapper.QueueBind(channel, queue.Name, exchange, routingKeyBinding, null);
                    }
                }
                else
                {
                    wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, null);
                }
            }
            else
            {
                wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, null);
            }
            
            result.Add(new QueueInfo
            {
                Exchange = exchange,
                Name = queue.Name,
                DeadLetterExchange = deadLetterExchange,
                ExchangeType = exchangeType,
                RetryExchange = retryExchange,
                RetryQueue = queue.RetryQueue.EmptyAlternative($"{exchange}-rtq"),
                PrefetchCount = queue.PrefetchCount ?? settings.DefaultPrefetchCount
            });
        }
        
        return result.ToArray();
    }
}

public interface IQueueSettings
{
    public string DefaultExchange { get; }
    
    /// <summary>
    /// Valid values are null, fanout, direct, topic and headers
    /// </summary>
    public string? DefaultExchangeType { get; }
    
    public int? DefaultPrefetchCount { get; }
    
    QueueSettings[]? Queues { get; }
}

public record QueueInfo
{
    public required string Name { get; init; }
    public required string Exchange { get; init; }
    public required string ExchangeType { get; init; }
    public string? DeadLetterExchange { get; init; }
    public string? RetryExchange { get; init; }
    public int? PrefetchCount { get; init; }
    public string? RetryQueue { get; set; }
}