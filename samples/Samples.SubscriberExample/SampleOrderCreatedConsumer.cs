using RelayPulse.Core;

namespace Samples.SubscriberExample;

public class SampleOrderCreatedConsumer : MessageConsumer<OrderCreated>
{
    private Random rnd = new Random();
    
    protected override async Task<ConsumerResponse> Consume(ConsumerInput<OrderCreated> input, CancellationToken ct)
    {
        if (input.RetryCount is >= 1 and  <= 2)
        {
            ConsumerResponse.TransientFailure("try again");
        }
        
        Console.WriteLine($"message handled by processor");

        var d = rnd.Next(1, 100);

        if (d > 50) return ConsumerResponse.TransientFailure("Api failed");
        
        return ConsumerResponse.Success();
    }

    public override bool IsApplicable(ConsumerInput input)
    {
        return input.Queue == "email-on-order-completed";
    }
}

public class OrderCreated
{
    public required string Id { get; init; }
    public string? Status { get; init; }
}