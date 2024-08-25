using RelayPulse.Core;
using RelayPulse.RabbitMQ;
using Samples.SubscriberExample;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddHostedService<Worker>();

builder.Services.AddRabbitMqRelayPulse(builder.Configuration);

builder.Services.AddScoped<IMessageProcessor, SampleOrderCreatedHandler>();

var host = builder.Build();
host.Run();