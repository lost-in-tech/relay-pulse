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
    
    
    public static void Expiry(this IDictionary<string, object> source, int? expiryInSeconds)
    {
        if(expiryInSeconds is null or 0) return;
        
        source[Constants.HeaderExpiryKey] = expiryInSeconds.Value * 1000;
    }

    public static int? Expiry(this IDictionary<string, object> source)
    {
        return source.TryGetValue(Constants.HeaderExpiryKey, out var value) ? (int)value/1000 : null;
    }
    
    public static void Expiry(this IDictionary<string, string> source, int? expiryInSeconds)
    {
        if(expiryInSeconds is null or 0) return;
        
        source[Constants.HeaderExpiryKey] = expiryInSeconds.Value.ToString("F0");
    }

    public static int? Expiry(this IDictionary<string, string> source)
    {
        return source.TryGetValue(Constants.HeaderExpiryKey, out var value) ? int.TryParse(value, out var intVal) ? intVal : null : null;
    }
    
    
    public static void RetryCount(this IDictionary<string, object> source, int retryCount)
    {
        if(retryCount == 0) return;
        
        source[Constants.HeaderRetryCount] = retryCount.ToString("F0");
    }

    public static int RetryCount(this IDictionary<string, object> source)
    {
        return source.TryGetValue(Constants.HeaderRetryCount, out var value) ? (int)value : 0;
    }
    
    public static void RetryCount(this IDictionary<string, string> source, int retryCount)
    {
        if(retryCount == 0) return;
        
        source[Constants.HeaderRetryCount] = retryCount.ToString("F0");
    }

    public static int RetryCount(this IDictionary<string, string> source)
    {
        return source.TryGetValue(Constants.HeaderRetryCount, out var value) ? int.TryParse(value, out var intVal) ? intVal : 0 : 0;
    }
}