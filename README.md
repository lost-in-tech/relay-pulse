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

```csharp
  "RelayPulse": {
    "RabbitMQ": {
      "Uri": "amqp://guest:guest@localhost:5672/",
      "DefaultExchange": "bookworm-events",
      "DefaultExchangeType": "direct"
      "AppId": null, //optional. this will be pass to message as appid
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
    Headers = new Dictionary<string,string> //optional
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
      "DefaultExchange": "bookworm-events", // optional for subscriber as subscriber can define exchange per queue. Or queue can override what define here
      "DefaultExchangeType": null, // optiona; for subscriber. default is "direct", valid values are fanout, direct, topic and headers
      "DefaultDeadLetterExchange": null, //optional
      "DefaultDeadLetterExchangeType": null, //optional default is direct. valid values direct or topic
      "DefaultRetryExchange": null, //optional
      "DefaultRetryExchangeType": null, //optional
      "DefaultPrefetchCount": null, //optional, default is 5,
      "Queues": [
        {
          "SkipSetup": null, // optional default is false. wont setup exchange or queue. just bind it
          "Name": "bookworm-email-receipt",
          "Exchange": null, // optional. fallback to defaultExchange value. Must need to provide here or defaultExchange value
          "ExchangeType": "topic", // optional, Valid values are null, fanout, direct, topic and headers
          "MsgExpiryInSeconds": null, //optional, default expiry of msg in queue
          "DeadLetterDisabled": false, //optional, default false. When true no deadletter and retry exchange will be used or created
          "DeadLetterExchange": null, // optional, if not define try to use DefaultDeadLetterExchange. if that one is empty then will use `<exchange>.dlx`
          "DeadLetterExchangeType": null, //optional, when null will try DefaultDeadLetterExchangeType otherwise defaulted to `direct`
          "DeadLetterRoutingKey": null, //optional. if not defined than queue name will be used as routing key
          "DeadLetterQueue": null, //optional. if not defined then `<queue name>.dlq` value will be used
          "RetryDisabled": false, //optional. by default retry enabled. but when deadletter disabled retry will be always disabled
          "RetryExchange": null, //optional. when empty will use `DefaultRetryExchange` and if that also empty then use `<exchange>.rtx` name
          "RetryExchangeType": null, //optional. when empty will use `DefaultRetryExchangeType` and then fallback to `direct` as default
          "DefaultRetryAfterInSeconds": null, //optional    
          "PrefetchCount": null, //optional. when null fallback to DefaultPrefetchCount
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
              "Headers": [
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

> *Note* Some useful header that used to pass information when publish message
> rp-retry-count, rp-msg-type, "rp-tenant", "rp-app-id"
> 
> You have the option to rename the header from settings. 

In example worker class we start listening as below:

```csharp
public class Worker(IMessageListener messageListener) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return messageListener.ListenUntilCancelled(stoppingToken);
    }
}
```

Now you need to write a handler as below which bound to a specific queue:

```csharp
// This handler will execute only when Message received from queue
public class SampleOrderCreatedHandler : MessageConsumer<OrderCreated>
{
    protected override async Task<ConsumerResponse> Consume(MessageProcessorInput<OrderCreated> input, CancellationToken ct)
    {
        .. process the message
        return MessageConsumerResponse.Success();
    }

    public override bool IsApplicable(ConsumerInput input)
    {
        return input.Queue == "email-on-order-completed";
    }
}
```

Make sure you register this handler in your DI. For example:

```csharp
services.TryAddEnumerable(ServiceDescription.Transient<IMessageConsumer,SampleOrderCreatedHandler>());
```

# Track message processing state

You can track and push data to other system. All you need to do is 
implement a notifier class against this interface and register in your IOC.
Example below:

```csharp
public class LogConsumeState(ILogger<LogConsumeState> logger) : INotifyConsumeState
{
    public Task Received(ConsumerInput input, CancellationToken ct = default)
    {
        logger.LogInfo("Receieved message to process with id {msgId}", input.MessageId);
    }

    public Task Processed(ConsumerInput input, ConsumerResponse response, CancellationToken ct = default)
    {
        logger.LogInfo("Completed processing of message to process with id {msgId} and {status}, {reason}", 
            input.MessageId, 
            response.Status, 
            response.Reason);
    }
}
```

Make sure you register this implementation in your IOC.

```csharp
services.TryAddEnumerable(ServiceDescriptor.Transient<LogConsumeState,INotifyConsumeState>());
```


# How to test Consumer

Say you have a consumer as below:

```csharp
public class SampleConsumer(IEmailClient emailClient) 
    : MessageConsumer<OrderCreated>
{
    protected override async Task<ConsumerResponse> Consume(
        ConsumerInput<T> input, 
        CancellationToken ct)
    {
        await emailClient.Send(...);
        
        return ConsumerResponse.Success();
    }
    
    public override IsApplicable(ConsumerInput input)
    {
        return input.Queue == "bookworms-email-on-order-completed";
    }
}
```

Now you can test his consumer as below using an extension method of IMessageConsumer
provided as part of `RelayPulse.Core`

```csharp
var givenFakeEmailClient = substitute.For<IEmailClient>(); // Create a fake email client
var givenInput = new ConsumerInput<OrderCreated>(new OrderCreated
    {
        Id = "order-123"
    });

var sut = new SampleConsumer(givneFakeEmailClient);

var gotRsp = await sut.Process(givenInput, CancellationToken.None);

gotRsp.Status.ShouldBe(MessageProcessStatus.Success);
```
