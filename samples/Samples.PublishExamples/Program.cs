// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RelayPulse.Core;
using RelayPulse.RabbitMQ;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .Build();



var sc = new ServiceCollection();
sc.AddRabbitMqRelayPulse(config);

var sp = sc.BuildServiceProvider();
var publisher = sp.GetRequiredService<IMessagePublisher>();
while (true)
{
    Console.WriteLine("enter order id");
    var input = Console.ReadLine();
    if(input == string.Empty || input == "x") break;

    
        
    
    await publisher.Publish(new Message<OrderCreated>
    {
        Id = Guid.NewGuid(),
        AppId = "sample-console",
        Content = new OrderCreated
        {
            Id = input!
        }
    });
}

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