using Bolt.Common.Extensions;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests.Helpers;

public static class ShouldlyExtensions
{
    public static void ShouldMatchContent<T>(this T source, string? key = null, string? customMsg = null)
    {
        (source.SerializeToPrettyJson() ?? string.Empty).ShouldMatchApproved((builder) =>
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                builder.WithDiscriminator(key);
            }
            builder.SubFolder("approved");
            builder.UseCallerLocation();
        }, customMsg);
    }
}