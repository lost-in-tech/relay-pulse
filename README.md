# relay-pulse
A generic pub sub client library

# How to use this library

Add `RelayPulse.RabbitMq` to your project

```xml
<PackageReference Include="RelayPulse.RabbitMQ" Version="..." />
```

Add following to add required implementation in your DI container.

```csharp
// you need to pass IConfiguration to AddRabbitMqRelayPulse
builder.Services.AddRabbitMqRelayPulse(builder.Configuration);
```

RabbitMQ settings loaded from configuration. you can define in 
appsettings or env or any other source e.g. AWS Parameter store

Sample appsettings as below:

```json
  "RelayPulse": {
    "RabbitMQ": {
      "Uri": "amqp://guest:guest@localhost:5672/",
      "DefaultExchange": "bookworm-events",
      "DefaultExchangeType": "direct"
    }
  }
```

> Note: It is advisable not to put sensitive information in appsettings or code. Use env variables or secrets. 


## Publish message to rabbitmq

Use IMessagePublisher to publish message. The following code will send
message to the default exchange defined in settings.

```csharp
public class OrderService(IMessagePublisher publisher, CancellationToken ct)
{
    public async Task CreateOrder(CreateOrderInput input)
    {
        .. save order
        
        await publisher.Publish(new OrderCreatedEvent
        {
            OrderId = "order-id"            
        }, ct);
    }
}
```

### Publish to specific exchange

```csharp
await publisher
    .Message(new OrderCreatedEvent{ ... })
    .Exchange("bookworm-order-exchange")
    .Publish();
```

### supply route key while publishing message

```csharp
await publisher
    .Message(new OrderCreatedEvent{...})
    .RouteKey("bookworm.order.created")
    .Publish();
```

### other data your can publish along with message

```csharp
await publisher.Publish(new Message<OrderCreatedEvent>
{
    Content = new OrderCreatedEvent
    {
        OrderId = "order-id
    },   
    Id = Guid.New(), // optional
    AppId = "your app id", // Optional
    UserId = "user-id", // Optional
    Cid = "correlation-id" // optional
    Type = "bookworm-order-created" // optional. default is name of T
    Tenant = "tenant-au" // optional
    Headers = new Dictionary<string,string>
    {
        ["custom-header"] = "custom-header-value"
    }
});
```

### Set common data for all message published

In an application you might send multiple type of messages from different places.
You might need to pass same data for all of them. e.g AppId, Tenant, CorrelationId etc.

Instead of adding in multiple places you ca write a filter that always update
all messages published with common data.

```csharp
public class SampleFilter : IMessageFilter
{
    public Message<T> Apply<T>(Message<T> msg)
    {
        return msg with 
        {
            AppId = "Appid",
            Cid = ... your code to get correlation id and set here
            ....
        }
    }
}    
```

Make sure you register this Filter class in your DI. for example

```csharp
services.TryAddEnumerable(ServiceDescriptor.Singleton<IMessageFilter,SampleFilter>());
```


# How to subscribe rabbitmq exchange

 For subscriber we need to define queues in settings as example below. In the following
example we defining two queues and bindings. One for exchange type topic and other is headers.
 
```csharp
"RelayPulse": {
    "RabbitMQ": {
      "Uri": "amqp://guest:guest@localhost:5672/",
      "DefaultExchange": "bookworm-events",
      "DefaultExchangeType": "direct",
      "Queues": [
        {
          "Name": "bookworm-email-receipt",
          "ExchangeType": "topic",
          "Bindings": [
            {
                "routingKey" : "order-created"
            }
          ]
        },
        {
          "Name": "bookworm-notify-slack",
          "ExchangeType": "headers",
          "Bindings": [
            {
              "MatchAny": true,
              "HeaderBindings": [
                {
                  "msg-type": "order-created"
                }
              ]
            }
          ]
        }
      ]
    }
  }
```

In example worker class we start listening as below:

```csharp
public class Worker : BackgroundService
{
    private readonly IMessageListener _listener;

    public Worker(IMessageListener listener)
    {
        _listener = listener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _listener.Init(stoppingToken);
        await _listener.Listen(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {

            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

Now you need to write a handler as below which bound to a specific queue:

```csharp
// This handler will execute only when Message received from queue
public class SampleOrderCreatedHandler : MessageProcessor<OrderCreated>
{
    protected override async Task<MessageProcessorResponse> Process(MessageProcessorInput<OrderCreated> input, CancellationToken ct)
    {
        .. process the message
        return MessageProcessorResponse.Success();
    }

    public override bool IsApplicable(MessageProcessorInput input)
    {
        return input.Queue == "email-on-order-completed";
    }
}
```

Make sure you register this handler in your DI. For example:

```csharp
services.TryAddEnumerable(ServiceDescription.Transient<IMessageProcessor,SampleOrderCreatedHandler>());
```