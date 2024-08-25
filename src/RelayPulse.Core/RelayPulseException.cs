namespace RelayPulse.Core;

public class RelayPulseException : ApplicationException
{
    public RelayPulseException(string message) : base(message) { }
    public RelayPulseException(string message, Exception innerException) : base(message, innerException){}
}