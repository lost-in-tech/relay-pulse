namespace RelayPulse.RabbitMQ;

public static class DictionaryExtensions
{
    public static string? PopValue(this Dictionary<string, string>? headers, string key)
    {
        if (headers == null) return null;
        var result = headers.GetValueOrDefault(key);
        if (result != null) headers.Remove(key);
        return result;
    }
    
    public static double? PopAsDouble(this Dictionary<string, string>? headers, string key)
    {
        var strValue = PopValue(headers, key);
        
        if (string.IsNullOrWhiteSpace(strValue)) return default;
        
        return double.TryParse(strValue, out var value) ? value : default;
    }
}