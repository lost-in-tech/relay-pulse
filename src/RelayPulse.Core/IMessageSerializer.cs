namespace RelayPulse.Core;


public interface IMessageSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(Stream value);
    object? Deserialize(Stream value, Type type);
}