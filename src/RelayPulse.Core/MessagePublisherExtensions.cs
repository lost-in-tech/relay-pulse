using RelayPulse.Core.Fluent;

namespace RelayPulse.Core;


public static class PublishMessageExtensions
{
    public static IHaveMessage Message<T>(this IMessagePublisher publisher, Guid id, T msg) =>
        new FluentMessagePublisher<T>(publisher, msg, id);
    
    public static IHaveMessage Message<T>(this IMessagePublisher publisher, T msg) =>
        new FluentMessagePublisher<T>(publisher, msg, null);
}