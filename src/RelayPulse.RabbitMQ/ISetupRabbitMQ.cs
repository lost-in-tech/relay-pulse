namespace RelayPulse.RabbitMQ;

public interface ISetupRabbitMq
{
    Task Run(CancellationToken ct);
}