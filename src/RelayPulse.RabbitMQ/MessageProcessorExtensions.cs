using System.Text;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

public static class MessageProcessorExtensions
{
    /// <summary>
    /// Extension method to make processor implementation test easy
    /// </summary>
    /// <param name="source"></param>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<MessageProcessorResponse> Process<T>(this IMessageProcessor source,
        MessageProcessorInput<T> input, CancellationToken ct = default)
    {
        var serializer = new MessageSerializer();
        var content = serializer.Serialize(input.Content);
        var contentArray = Encoding.UTF8.GetBytes(content);
        using var ms = new MemoryStream(contentArray);

        return source.Process(input, ms, serializer, ct);
    }
}