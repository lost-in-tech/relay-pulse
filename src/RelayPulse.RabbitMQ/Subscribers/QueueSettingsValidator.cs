using RabbitMQ.Client;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal sealed class QueueSettingsValidator
{
    public void Validate(IQueueSettings settings)
    {
        if (settings.Queues == null || settings.Queues.Length == 0)
        {
            throw new RelayPulseException("No queues settings provided");
        }

        foreach (var queue in settings.Queues)
        {
           Validate(settings, queue);
        }
    }

    private static readonly string[] ExchangeTypesSupported = [RabbitMQ.ExchangeTypesSupported.Fanout,
        RabbitMQ.ExchangeTypesSupported.Direct,
        RabbitMQ.ExchangeTypesSupported.Topic,
        RabbitMQ.ExchangeTypesSupported.Headers];
    private void Validate(IQueueSettings settings, QueueSettings queue)
    {
        var exchangeName = queue.Exchange.EmptyAlternative(settings.DefaultExchange);

        if (string.IsNullOrWhiteSpace(exchangeName))
        {
            throw new RelayPulseException(
                $"Exchange name cannot be empty. Provide an exchange name for the queue {queue.Name}");
        }

        var exchangeType = queue.ExchangeType.TryPickNonEmpty(settings.DefaultExchangeType);

        if (string.IsNullOrWhiteSpace(exchangeType))
        {
            throw new RelayPulseException($"Exchange type cannot be empty. Provide an exchange type for queue {queue.Name}");
        }

        if (ExchangeTypesSupported.All(xc => xc != exchangeType))
        {
            throw new RelayPulseException(
                $"Exchange type not valid. Provide a valid exchange type value. Supported values are {string.Join(", ",ExchangeTypesSupported)}");
        }

        if (exchangeType.IsSame(ExchangeType.Fanout))
        {
            if (queue.Bindings is { Length: > 0 })
            {
                throw new RelayPulseException($"Bindings not supported for exchange type {exchangeType}");
            }
        }
    }
}