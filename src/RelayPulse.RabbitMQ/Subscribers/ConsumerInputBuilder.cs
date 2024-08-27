using System.Globalization;
using RabbitMQ.Client.Events;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ.Subscribers;

internal static class ConsumerInputBuilder
{
    public static ConsumerInput  Build(QueueInfo queueInfo, BasicDeliverEventArgs args)
    {
        return new ConsumerInput
        {
            Queue = queueInfo.Name,
            Type = args.BasicProperties.Type,
            Cid = args.BasicProperties.CorrelationId,
            Id = args.BasicProperties.MessageId,
            Tenant = GetHeaderValue(args,Constants.HeaderTenant),
            AppId = args.BasicProperties.AppId,
            UserId = args.BasicProperties.UserId,
            SentAt = DateTime.TryParse(GetHeaderValue(args, Constants.HeaderSentAt), null, DateTimeStyles.RoundtripKind, out var dt) ? dt : null,
            RetryCount = args.BasicProperties.Headers?.RetryCount() ?? 0,
            Headers = ToHeaders(args),
        };
    }

    private static Dictionary<string, string> ToHeaders(BasicDeliverEventArgs args)
    {
        var result = new Dictionary<string, string>();

        if (args.BasicProperties.Headers == null) return result;

        foreach (var header in args.BasicProperties.Headers)
        {
            var value = header.Value?.ToString();
            
            if(value == null) continue;

            result[header.Key] = value;
        }

        return result;
    }

    private static string? GetHeaderValue(BasicDeliverEventArgs args, string header)
    {
        var headers = args.BasicProperties.Headers;
        if (headers == null) return null;
        return headers.TryGetValue(header, out var value) ? value?.ToString() : null;
    }
}