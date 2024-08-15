using System.Text.Json;
using System.Text.Json.Serialization;

namespace RelayPulse.RabbitMQ;

public interface IMessageSerializer
{
    string Serialize<T>(T value);
}

internal sealed class MessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public MessageSerializer()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new JsonStringEnumConverter());
        _options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
    
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}