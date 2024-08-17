﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RelayPulse.Core;
using RelayPulse.RabbitMQ;

var config = new ConfigurationBuilder().Build();
    
var sc = new ServiceCollection();
sc.AddRabbitMqRelayPulse(config, new RabbitMqRelayHubOptions
{
    Settings = new RabbitMqSettings
    {
        Uri = "amqp://guest:guest@localhost:5672/",
        DefaultExchange = "bookworm-events"
    }
});

var sp = sc.BuildServiceProvider();

var publisher = sp.GetRequiredService<IMessagePublisher>();

await publisher.Message(new OrderCreated
{
    Id = "order-123"
}).Tenant("au").Expiry(10).AppId("api-bookworm").Publish();

await publisher.Message(new OrderCancelled
{
    Id = "order-124"
}).Tenant("au").Expiry(10).AppId("api-bookworm").Publish();

await publisher.Message(new OrderLost
{
    Id = "order-124"
}).Tenant("au").Expiry(10).AppId("api-bookworm").Publish();

Console.WriteLine("Hello, World!");

public record OrderCreated
{
    public required string Id { get; init; }
}

public record OrderCancelled
{
    public required string Id { get; init; }
}



public record OrderLost
{
    public required string Id { get; init; }
}