namespace RelayPulse.Core;

public interface IMessageListener
{
    Task Init(CancellationToken ct);
    Task Listen(CancellationToken ct);
}