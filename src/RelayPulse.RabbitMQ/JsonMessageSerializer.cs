using System.Text.Json;
using System.Text.Json.Serialization;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;


internal sealed class MessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public MessageSerializer()
    {
        _options = new JsonSerializerOptions();
        _options.PropertyNameCaseInsensitive = true;
        _options.Converters.Add(new JsonStringEnumConverter());
        _options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
    
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    public T Deserialize<T>(Stream value)
    {
        return JsonSerializer.Deserialize<T>(value, _options)!;
    }

    public object? Deserialize(Stream value, Type type)
    {
        return JsonSerializer.Deserialize(value, type, _options)!;
    }
}