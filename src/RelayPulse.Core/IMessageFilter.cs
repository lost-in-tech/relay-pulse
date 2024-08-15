namespace RelayPulse.Core;

public interface IMessageFilter
{
    Message<T> Apply<T>(Message<T> msg);
}