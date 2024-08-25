using RelayPulse.Core;
using Shouldly;

namespace RelayPulse.RabbitMQ.Tests;

public partial class MessagePublisherTests
{
    [Fact]
    public void publish_throw_exception_when_exchange_name_missing_in_input_and_also_in_settings()
    {
        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var givenSettingsDoesntProvideExchangeName = new RabbitMqSettings();

        Should.Throw<RelayPulseException>(async () => await Execute(new Message<OrderCreated>
        {
            Content = givenMsg
        }, givenSettingsDoesntProvideExchangeName));
    }
    
    [Fact]
    public void publish_not_throw_exception_when_exchange_name_provided_in_settings_but_not_in_input()
    {
        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var givenSettingsDoesntProvideExchangeName = new RabbitMqSettings
        {
            DefaultExchange = "test-exchange"
        };

        Should.NotThrowAsync(async () => await Execute(new Message<OrderCreated>
        {
            Content = givenMsg
        }, givenSettingsDoesntProvideExchangeName));
    }
    
    [Fact]
    public void publish_not_throw_exception_when_exchange_name_provided_in_input_but_not_in_settings()
    {
        var givenMsg = new OrderCreated
        {
            Id = "123"
        };

        var givenSettingsDoesntProvideExchangeName = new RabbitMqSettings();

        Should.NotThrowAsync(async () => await Execute(new Message<OrderCreated>
        {
            Content = givenMsg
        }.Exchange("test-exchange"), givenSettingsDoesntProvideExchangeName));
    }
}