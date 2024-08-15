namespace RelayPulse.RabbitMQ.Tests.Fakes;

internal sealed class FakeUniqueId : IUniqueId
{
    public Guid New() => Constants.FixedGuidOne;
}