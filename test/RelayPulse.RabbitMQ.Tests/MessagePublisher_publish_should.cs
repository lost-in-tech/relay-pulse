using RelayPulse.Core;
using RelayPulse.RabbitMQ.Tests.Fakes;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public class MessagePublisher_publish_should(IocFixture fixture) : IClassFixture<IocFixture>
{
    [Fact]
    public async Task send_correct_payload_to_rabbit_mq_when_only_event_payload_provided()
    {
        var sut = fixture.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };
        
        var rsp = await sut.Publish(givenMsg);

        rsp.ShouldBeTrue();

        var gotCallInfo = fixture.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotCallInfo.ShouldSatisfyAllConditions
        (
            () => gotCallInfo.ExecutionCount.ShouldBe(1),
            () => gotCallInfo.LastInput!.Exchange.ShouldBe("bookworm.events"),
            () => gotCallInfo.LastInput!.BasicProperties.Type.ShouldBe(typeof(OrderCreated).FullName),
            () => gotCallInfo.LastInput!.BasicProperties.MessageId.ShouldBe(Constants.FixedGuidOne.ToString()),
            () => gotCallInfo.Body!.Id.ShouldBe("123")
        );
    }

    public record OrderCreated
    {
        public required string Id { get; init; }
    }
}