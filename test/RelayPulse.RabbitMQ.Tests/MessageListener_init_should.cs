using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelayPulse.Core;
using RelayPulse.RabbitMQ.Subscribers;
using RelayPulse.RabbitMQ.Tests.Fakes;
using RelayPulse.RabbitMQ.Tests.Helpers;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public class MessageListener_init_should(IocFixture fixture) : IClassFixture<IocFixture>
{
    [Fact]
    public async Task Setup_exchange_and_queues_when_queue_settings_empty()
    {
        var sp = fixture.Services();
        var sut = sp.GetRequiredService<ISetupRabbitMq>();
        await Should.ThrowAsync<RelayPulseException>(async () =>  await sut.Run(CancellationToken.None));
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Setup_exchange_and_queues(TestInput<RabbitMqSettings> input)
    {
        var sp = fixture.Services(sc => { sc.Replace(ServiceDescriptor.Singleton<IQueueSettings>(input.Data)); });
        var sut = sp.GetRequiredService<ISetupRabbitMq>();
        await sut.Run(CancellationToken.None);
        var gotWrapper = sp.GetRequiredService<IRabbitMqWrapper, FakeRabbitMqWrapper>();

        new
        {
            gotWrapper.ExchangeDeclareCalls,
            gotWrapper.QueueDeclareCalls,
            gotWrapper.QueueBindCalls
        }.ShouldMatchContent(input.Key, input.Scenario);
    }

    public static IEnumerable<object[]> TestData = new TestInput<RabbitMqSettings>[]
    {
        new()
        {
            Key = "1",
            Scenario = "When default exchange is fanout",
            Data = new RabbitMqSettings
            {
                DefaultExchange = "default-exchange",
                DefaultExchangeType = ExchangeTypesSupported.Fanout,
                Queues =
                [
                    new QueueSettings
                    {
                        Name = "queue-fanout",
                    }
                ]
            }
        },
        new()
        {
            Key = "2",
            Scenario = "When exchange is fanout and name provided as part of queue",
            Data = new RabbitMqSettings
            {
                DefaultExchange = "default-exchange",
                DefaultExchangeType = ExchangeTypesSupported.Headers,
                Queues =
                [
                    new QueueSettings
                    {
                        Exchange = "non-default-exchange",
                        ExchangeType = ExchangeTypesSupported.Fanout,
                        Name = "queue-fanout"
                    }
                ]
            }
        },
        new()
        {
            Key = "3",
            Scenario = "When exchange is topic and name provided as part of queue",
            Data = new RabbitMqSettings
            {
                Queues =
                [
                    new QueueSettings
                    {
                        Exchange = "non-default-exchange",
                        ExchangeType = ExchangeTypesSupported.Topic,
                        Name = "queue-topic",
                        Bindings =
                        [
                            new QueueBinding
                            {
                                RoutingKey = "test-route-key"
                            }
                        ]
                    }
                ]
            }
        },
        new()
        {
            Key = "4",
            Scenario = "When exchange is header and name provided as part of queue",
            Data = new RabbitMqSettings
            {
                Queues =
                [
                    new QueueSettings
                    {
                        Exchange = "non-default-exchange",
                        ExchangeType = ExchangeTypesSupported.Headers,
                        Name = "queue-header",
                        Bindings =
                        [
                            new QueueBinding
                            {
                                MatchAny = true,
                                Headers = new Dictionary<string, string>{
                                    ["event-name"] = "order-created"
                                }
                            }
                        ]
                    }
                ]
            }
        },
        new()
        {
            Key = "5",
            Scenario = "a real world example",
            Data = new RabbitMqSettings
            {
                DefaultExchange = "bookworm.orders",
                DefaultExchangeType = ExchangeTypesSupported.Topic,
                Queues = [
                    new QueueSettings
                    {
                        Name = "bookworm.order-created.email",
                        Bindings = new []
                        {
                            new QueueBinding
                            {
                                RoutingKey = "Bookworm.OrderCreated"
                            }
                        }
                    },
                    new QueueSettings
                    {
                        Name = "bookworm.order-created.slack",
                        Bindings = new []
                        {
                            new QueueBinding
                            {
                                RoutingKey = "Bookworm.OrderCreated"
                            }
                        }
                    }
                ]
            }
        }
    }.ToTestData();
}