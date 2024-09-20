using RelayPulse.Core;
using RelayPulse.RabbitMQ;
using Samples.SubscriberExample;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddRabbitMqRelayPulse(builder.Configuration);

builder.Services.AddScoped<IMessageConsumer, SampleOrderCreatedConsumer>();
builder.Services.AddScoped<INotifyConsumeState, NewRelicConsumeState>();

var host = builder.Build();

host.Run();