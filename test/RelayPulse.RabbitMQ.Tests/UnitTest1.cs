using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using RabbitMQ.Client;
using RelayPulse.Core;
using RelayPulse.RabbitMQ.Tests.Fakes;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public class MessagePublisherTests(IocFixture fixture) : IClassFixture<IocFixture>
{
    [Fact]
    public async Task Publish_should_send_correct_payload_to_rabbit_mq()
    {
        var sut = fixture.GetRequiredService<IMessagePublisher>();

        var rsp = await sut.Content(new { Id = "123" })
            .Publish();
        
        rsp.ShouldBeTrue();
        var gotInput = fixture.GetRequiredService<IRabbitMqWrapper, FakeRabbitMqWrapper>();
        gotInput.LastInput.ShouldNotBeNull();
        gotInput.LastInput.Exchange.ShouldBe("bookworm.events");
        gotInput.TotalExecutionCount.ShouldBe(1);
    }

    public record OrderCreated
    {
        public string Id { get; init; }
    }
}