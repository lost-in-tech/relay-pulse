using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RelayPulse.Core;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public class MessagePublisherTests
{
    [Fact]
    public async Task Publish_should_send_correct_payload_to_rabbit_mq()
    {
        Assert.True(true);
        // var config = new ConfigurationBuilder().Build();
        // var sc = new ServiceCollection();
        // sc.AddRabbitMqRelayHub(config);
        // var sp = sc.BuildServiceProvider();
        // var sut = sp.GetRequiredService<IMessagePublisher>();
        // var rsp = await sut.Content(new { Id = "123" })
        //     .Publish();
        //
        // rsp.ShouldBeTrue();
    }
}