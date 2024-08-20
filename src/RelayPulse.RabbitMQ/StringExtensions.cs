using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace RelayPulse.RabbitMQ;

internal static class StringExtensions
{
    private static readonly Regex CamelCaseToSnakeCaseRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);

    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToSnakeCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        
        string snakeCase = CamelCaseToSnakeCaseRegex.Replace(value, "$1-$2").ToLowerInvariant();

        return snakeCase;
    }

    public static string Join(this IEnumerable<string?> source, string separator)
    {
        var sb = new StringBuilder();

        foreach (var item in source)
        {
            if(string.IsNullOrWhiteSpace(item)) continue;
            
            sb.Append(separator).Append(item);
        }

        var result = sb.ToString();

        return result.Length == 0 ? result : result.Substring(1);
    }
    
    public static bool HasValue([NotNullWhen(true)]this string? value) => !string.IsNullOrWhiteSpace(value);
    
    public static string NullToEmpty(this string? value) => value ?? string.Empty;
    
    public static string EmptyAlternative(this string? value, string alternative)
    {
        return string.IsNullOrWhiteSpace(value) ? alternative : value;
    }
    
    public static string? TryPickNonEmpty(this string? value, params string?[] alternatives)
    {
        if (!string.IsNullOrWhiteSpace(value)) return value;

        foreach (var alternative in alternatives)
        {
            if (!string.IsNullOrWhiteSpace(alternative)) return alternative;
        }

        return null;
    }
}
