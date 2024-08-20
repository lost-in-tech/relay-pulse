using RelayPulse.Core;
using RelayPulse.RabbitMQ;
using Samples.SubscriberExample;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddHostedService<Worker>();

builder.Services.AddRabbitMqRelayPulse(builder.Configuration, new RabbitMqRelayHubOptions
{
    Settings = new RabbitMqSettings
    {
        Uri = "amqp://guest:guest@localhost:5672/",
        DefaultExchange = "bookworm-events",
        DefaultExchangeType = "direct",
        Queues =
        [
            new QueueSettings
            {
                Name = "email-on-order-completed"
            }
        ]
    }
});

builder.Services.AddScoped<IMessageProcessor, SampleOrderCreatedHandler>();

var host = builder.Build();
host.Run();