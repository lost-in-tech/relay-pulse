using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using RelayPulse.Core;
using RelayPulse.RabbitMQ.Tests.Fakes;
using RelayPulse.RabbitMQ.Tests.Helpers;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public class MessagePublisher_publish_should(IocFixture fixture) : IClassFixture<IocFixture>
{
    [Fact]
    public async Task send_correct_payload_to_rabbit_mq_when_only_event_payload_provided()
    {
        // Arrange
        var services = fixture.Services();

        var sut = services.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        // Act
        var rsp = await sut.Publish(givenMsg);
        var gotCallInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        // Assert
        rsp.ShouldBeTrue();

        gotCallInfo.ShouldSatisfyAllConditions
        (
            () => gotCallInfo.ExecutionCount.ShouldBe(1),
            () => gotCallInfo.LastInput!.Exchange.ShouldBe("bookworm.events"),
            () => gotCallInfo.LastInput!.BasicProperties.Type.ShouldBe(typeof(OrderCreated).FullName),
            () => gotCallInfo.LastInput!.BasicProperties.MessageId.ShouldBe(Constants.FixedGuidOne.ToString()),
            () => gotCallInfo.Body!.Id.ShouldBe("123")
        );
    }

    [Fact]
    public async Task use_expiry_when_provided()
    {
        var services = fixture.Services();

        var sut = services.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };


        var rsp = await sut.Message(givenMsg).Expiry(5).Publish();
        var callInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        rsp.ShouldBeTrue();

        callInfo.LastInput!.BasicProperties.Expiration.ShouldBe("5000");
    }

    [Fact]
    public async Task use_expiry_when_default_expiry_provided()
    {
        var services = fixture.Services(x =>
        {
            x.Replace(ServiceDescriptor.Singleton<IMessagePublishSettings>(RabbitMqSettingsBuilder.Build() with
            {
                DefaultExpiryInSeconds = 10
            }));
        });

        var sut = services.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };


        var rsp = await sut.Message(givenMsg).Publish();
        var callInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        rsp.ShouldBeTrue();

        callInfo.LastInput!.BasicProperties.Expiration.ShouldBe("10000");
    }

    [Fact]
    public async Task use_prefix_for_type_name_when_provided()
    {
        var services = fixture.Services(x =>
        {
            x.Replace(ServiceDescriptor.Singleton<IMessagePublishSettings>(RabbitMqSettingsBuilder.Build() with
            {
                TypePrefix = "test-"
            }));
        });

        var sut = services.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var rsp = await sut.Publish(givenMsg);
        var callInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        rsp.ShouldBeTrue();

        callInfo.LastInput!.BasicProperties.Type.ShouldBe(typeof(OrderCreated).FullName ?? nameof(OrderCreated));
        callInfo.LastInput!.BasicProperties.Headers.ShouldContainKey(RabbitMQ.Constants.HeaderMsgTypeFull);
    }

    [Fact]
    public async Task use_exchange_that_provided_in_input()
    {
        var services = fixture.Services();

        var sut = services.GetRequiredService<IMessagePublisher>();


        var givenExchange = "test-exchange";
        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var rsp = await sut.Message(givenMsg)
            .Exchange(givenExchange)
            .Publish();

        rsp.ShouldBeTrue();

        var gotCallInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotCallInfo.LastInput!.Exchange.ShouldBe(givenExchange);
    }

    [Fact]
    public async Task use_routing_key_that_provided_in_input()
    {
        var services = fixture.Services();

        var sut = services.GetRequiredService<IMessagePublisher>();

        var giveRoutingKey = "test-route-key";
        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var rsp = await sut.Message(givenMsg)
            .Routing(giveRoutingKey)
            .Publish();

        rsp.ShouldBeTrue();

        var gotCallInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotCallInfo.LastInput!.RoutingKey.ShouldBe(giveRoutingKey);
    }

    [Fact]
    public async Task use_message_id_that_provided_in_input()
    {
        var services = fixture.Services();

        var sut = services.GetRequiredService<IMessagePublisher>();

        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var rsp = await sut
            .Message(Constants.FixedGuidTwo, givenMsg)
            .Publish();

        rsp.ShouldBeTrue();

        var gotCallInfo = services.GetRabbitMqPublishCallInfo<OrderCreated>();

        gotCallInfo.LastInput!.BasicProperties.MessageId.ShouldBe(Constants.FixedGuidTwo.ToString());
    }

    [Fact]
    public async Task use_value_provided_by_filter()
    {
        var givenMsg = new Message<OrderCreated>
        {
            Cid = "MyCidThatShouldOverride",
            Content = new OrderCreated { Id = "123" }
        };
        
        var givenFilter = Substitute.For<IMessageFilter>();
        var givenFilterMsg = givenMsg with
        {
            Id = new Guid("D1438A72-DAB3-41B8-B3DD-1F5ABD7713E3"),
            Cid = "CidFiltered",
            Type = "TypeFiltered",
            AppId = "AppIdFiltered",
            UserId = "UserIdFiltered",
            Tenant = "TenantFiltered"
        };
        givenFilterMsg.Headers["item-filter"] = "itemFilterValue";
        
        givenFilter.Apply(givenMsg).Returns(givenFilterMsg);
        
        var gotBasicInput = await Execute(
            givenMsg: givenMsg,
            settings: null,
            filters: [givenFilter]);

        gotBasicInput.ShouldNotBeNull();
        gotBasicInput.BasicProperties.ShouldMatchContent();
    }

    private async Task<BasicPublishInput?> Execute<T>(
        Message<T> givenMsg,
        RabbitMqSettings? settings = null,
        IEnumerable<IMessageFilter>? filters = null)
    {
        var services = fixture.Services(sc =>
        {
            if (settings != null)
            {
                sc.Replace(ServiceDescriptor.Singleton<IMessagePublishSettings>(settings));
            }

            if (filters != null)
            {
                foreach (var messageFilter in filters)
                {
                    sc.AddTransient<IMessageFilter>(_ => messageFilter);
                }
            }
        });

        var sut = services.GetRequiredService<IMessagePublisher>();

        _ = await sut.Publish(givenMsg);

        var gotCallInfo = services.GetRabbitMqPublishCallInfo<T>();

        return gotCallInfo.LastInput;
    }

    public record OrderCreated
    {
        public required string Id { get; init; }
    }
}