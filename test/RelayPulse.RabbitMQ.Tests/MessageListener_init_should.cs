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
        var sut = sp.GetRequiredService<IMessageListener>();
        await Should.ThrowAsync<RelayPulseException>(async () =>  await sut.Init(CancellationToken.None));
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Setup_exchange_and_queues(TestInput<RabbitMqSettings> input)
    {
        var sp = fixture.Services(sc => { sc.Replace(ServiceDescriptor.Singleton<IQueueSettings>(input.Data)); });
        var sut = sp.GetRequiredService<IMessageListener>();
        await sut.Init(CancellationToken.None);
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
                        RetryFeatureDisabled = true
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
                        RetryQueue = "test-route-key",
                        Name = "queue-fanout",
                        RetryFeatureDisabled = true
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
                        RetryQueue = "retry-queue",
                        Name = "queue-topic",
                        RetryFeatureDisabled = false,
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
                        RetryQueue = "retry-queue",
                        Name = "queue-topic",
                        RetryFeatureDisabled = false,
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
        }
    }.ToTestData();
}