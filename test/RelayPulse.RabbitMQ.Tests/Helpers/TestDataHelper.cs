namespace RelayPulse.RabbitMQ.Tests.Helpers;

public static class TestDataHelper
{
    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<TestInput<T>> source)
    {
        var index = 0;
        foreach (var item in source)
        {
            yield return [item with
            {
                Key = string.IsNullOrWhiteSpace(item.Key) ? $"{index}" : item.Key
            }];
        }
    }
    
    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<T> source)
    {
        var index = 0;
        foreach (var item in source)
        {
            yield return [new TestInput<T>
            {
                Key = $"key-{index}",
                Data = item
            }];
        }
    }
}

public record TestInput<T>
{
    public string? Key { get; init; }
    public string? Scenario { get; init; }
    public required T Data { get; init; }
    
    public static implicit operator T(TestInput<T> input) => input.Data;
    public static implicit operator TestInput<T>(T input) => new()
    {
        Data = input
    };
}