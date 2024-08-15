namespace RelayPulse.RabbitMQ;

internal sealed class UniqueId : IUniqueId
{
    public Guid New() => Guid.NewGuid();
}

internal interface IUniqueId
{
    Guid New();
}