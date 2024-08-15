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

        var rsp = await sut.Content(new OrderCreated
            {
                Id = "123"
            })
            .Publish();

        rsp.ShouldBeTrue();

        var (count, gotPublishInput) = fixture.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotPublishInput.ShouldSatisfyAllConditions
        (
            () => count.ShouldBe(1),
            () => gotPublishInput.Exchange.ShouldBe("bookworm.events"),
            () => gotPublishInput.Type.ShouldBe(typeof(OrderCreated).FullName),
            () => gotPublishInput.MsgId.ShouldNotBeEmpty(),
            () => gotPublishInput.Body.Id.ShouldBe("123")
        );
    }

    public record OrderCreated
    {
        public string Id { get; init; }
    }
}