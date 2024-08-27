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
                .EmptyAlternative(ExchangeTypesSupported.Direct);

            var deadLetterExchange = queue.DeadLetterDisabled
                ? null
                : queue.DeadLetterExchange.EmptyAlternative(DefaultDeadLetterExchange(exchange));
            var deadLetterQueue = queue.DeadLetterDisabled
                ? null
                : queue.DeadLetterQueue.EmptyAlternative(DefaultDeadLetterQueue(exchange));
            

            var retryExchange = queue.RetryFeatureDisabled
                ? null
                : queue.RetryExchange.EmptyAlternative(DefaultRetryExchange(exchange));
            var retryQueue = queue.RetryFeatureDisabled
                ? null
                : queue.RetryQueue.EmptyAlternative(DefaultRetryQueue(exchange));
            
            if (queue.SkipSetup ?? false)
            {
                result.Add(new QueueInfo
                {
                    Exchange = exchange,
                    Name = queue.Name,
                    ExchangeType = exchangeType,
                    RetryExchange = retryExchange,
                    RetryQueue = retryQueue,
                    DeadLetterExchange = deadLetterExchange
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

            if (deadLetterExchange.HasValue()
                && deadLetterQueue.HasValue())
            {
                if (!exchangeCreated.ContainsKey(deadLetterExchange))
                {
                    wrapper.ExchangeDeclare(channel, deadLetterExchange, ExchangeTypesSupported.Topic);
                
                    exchangeCreated[deadLetterExchange] = deadLetterExchange;
                }

                wrapper.QueueDeclare(channel, deadLetterQueue, null);
                
                wrapper.QueueBind(channel, deadLetterQueue, deadLetterExchange, "*", null);

                queueDeclareArgs[Constants.HeaderDeadLetterExchange] = deadLetterExchange;
            }


            if (retryExchange.HasValue() && retryQueue.HasValue())
            {
                if (!exchangeCreated.ContainsKey(retryExchange))
                {
                    wrapper.ExchangeDeclare(channel, retryExchange, ExchangeTypesSupported.Topic);
                
                    exchangeCreated[retryExchange] = retryExchange;
                }

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

            var queueBinding = queue.Bindings ?? [];

            if (exchangeType == ExchangeTypesSupported.Headers)
            {
                if (!queue.RetryFeatureDisabled)
                {
                    wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, new Dictionary<string, object>
                    {
                        [Constants.HeaderMatch] = "all",
                        [Constants.HeaderTargetQueue] = queue.Name
                    });
                }

                foreach (var binding in queueBinding)
                {
                    var bindingArgs = new Dictionary<string, object>();

                    var headersToBind = binding.Headers;

                    if(headersToBind == null || headersToBind.Count == 0) continue;
                    
                    foreach (var headerToBind in headersToBind)
                    {
                        bindingArgs[headerToBind.Key] = headerToBind.Value;
                    }

                    if (binding.MatchAny ?? false)
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
            else if (exchangeType != ExchangeTypesSupported.Fanout)
            {
                if (!queue.RetryFeatureDisabled)
                {
                    wrapper.QueueBind(channel, queue.Name, exchange, Constants.RouteKeyTargetQueue(queue.Name), null);
                }

                foreach (var binding in queueBinding)
                {
                    if (binding.RoutingKey.HasValue())
                    {
                        wrapper.QueueBind(channel, queue.Name, exchange, binding.RoutingKey, null);
                    }
                    else
                    {
                        wrapper.QueueBind(channel, queue.Name, exchange, string.Empty, null);
                    }
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
                RetryQueue = retryQueue,
                PrefetchCount = queue.PrefetchCount ?? settings.DefaultPrefetchCount
            });
        }
        
        return result.ToArray();
    }

    private string DefaultDeadLetterExchange(string exchange) => $"{exchange}-dlx";
    private string DefaultDeadLetterQueue(string exchange) => $"{exchange}-dlq";
    private string DefaultRetryExchange(string exchange) => $"{exchange}-rtx";
    private string DefaultRetryQueue(string exchange) => $"{exchange}-rtq";
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
    public string Name { get; set; } = String.Empty;
    public string Exchange { get; set; } = String.Empty;
    public string ExchangeType { get; set; } = String.Empty;
    public string? DeadLetterExchange { get; set; }
    public string? RetryExchange { get; set; }
    public int? PrefetchCount { get; set; }
    public string? RetryQueue { get; set; }
}