namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class QueueSettingsValidator
{
    public void Validate(IQueueSettings settings)
    {
        if(settings.Queues == null || settings.Queues.Length == 0) return;

        foreach (var queue in settings.Queues)
        {
           Validate(settings, queue);
        }
    }

    private static readonly string[] ValidExchangeTypes = [Constants.ExchangeTypeFanout,
        Constants.ExchangeTypeDirect,
        Constants.ExchangeTypeTopic,
        Constants.ExchangeTypeHeader];
    private void Validate(IQueueSettings settings, QueueSettings queue)
    {
        var exchangeName = queue.Exchange.EmptyAlternative(settings.DefaultExchange);

        if (string.IsNullOrWhiteSpace(exchangeName))
        {
            throw new Exception(
                $"Exchange name cannot be empty. Provide an exchange name for the queue {queue.Name}");
        }

        var exchangeType = queue.ExchangeType.TryPickNonEmpty(settings.DefaultExchangeType);

        if (string.IsNullOrWhiteSpace(exchangeType))
        {
            throw new Exception($"Exchange type cannot be empty. Provide an exchange type for queue {queue.Name}");
        }

        if (ValidExchangeTypes.All(xc => xc != exchangeType))
        {
            throw new Exception(
                $"Exchange type not valid. Provide a valid exchange type value. Supported values are {string.Join(", ",ValidExchangeTypes)}");
        }

        if (!queue.RetryFeatureDisabled)
        {
            if (exchangeType == Constants.ExchangeTypeFanout)
            {
                throw new Exception(
                    $"Retry feature is not supported for fanout exchange. Either change the exchange type or make retry feature disable. query {queue.Name}");
            }

            if (exchangeType == Constants.ExchangeTypeHeader)
            {
                var totalBindings = queue.Binding?.HeaderBindings?.Count() ?? 0;

                if (totalBindings == 0)
                {
                    throw new Exception($"For retry feature to work At least one header binding is required. query {queue.Name}");
                }
                
                var anyEmptyArgs = queue.Binding?.HeaderBindings?.Any(x => x.Args.Count == 0) ?? false;

                if (anyEmptyArgs)
                    throw new Exception(
                        $"Header bindings with empty headers not supported when retry feature is enabled for query {queue.Name}");
            }
        }
    }
}