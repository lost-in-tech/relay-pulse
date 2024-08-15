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

        var gotCallInfo = fixture.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotCallInfo.ShouldSatisfyAllConditions
        (
            () => gotCallInfo.ExecutionCount.ShouldBe(1),
            () => gotCallInfo.LastInput!.Exchange.ShouldBe("bookworm.events"),
            () => gotCallInfo.LastInput!.BasicProperties.Type.ShouldBe(typeof(OrderCreated).FullName),
            () => gotCallInfo.LastInput!.BasicProperties.MessageId.ShouldNotBeEmpty(),
            () => gotCallInfo.Body!.Id.ShouldBe("123")
        );
    }

    public record OrderCreated
    {
        public string Id { get; init; }
    }
}