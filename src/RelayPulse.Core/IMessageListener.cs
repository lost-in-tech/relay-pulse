namespace RelayPulse.Core;

public interface IMessageListener
{
    Task Listen(CancellationToken ct);
}