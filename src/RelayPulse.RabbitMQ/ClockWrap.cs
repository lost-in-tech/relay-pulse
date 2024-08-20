namespace RelayPulse.RabbitMQ;

public interface IClockWrap
{
    DateTime UtcNow { get; }
}

internal sealed class ClockWrap : IClockWrap
{
    public DateTime UtcNow => DateTime.UtcNow;
}