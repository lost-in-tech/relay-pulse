using RelayPulse.Core;

namespace Samples.SubscriberExample;

public class SampleOrderCreatedHandler : MessageProcessor<OrderCreated>
{
    private Random rnd = new Random();
    
    protected override async Task<MessageProcessorResponse> Process(MessageProcessorInput<OrderCreated> input, CancellationToken ct)
    {
        if (input.RetryCount.HasValue && input.RetryCount.Value is  >= 1 and  <= 2)
        {
            MessageProcessorResponse.TransientFailure("try again");
        }
        
        Console.WriteLine($"message handled by processor");

        var d = rnd.Next(1, 100);

        if (d > 50) return MessageProcessorResponse.TransientFailure("Api failed");
        
        return MessageProcessorResponse.Success();
    }

    public override bool IsApplicable(MessageProcessorInput input)
    {
        return input.Queue == "email-on-order-completed";
    }
}

public class OrderCreated
{
    public required string Id { get; init; }
    public string? Status { get; init; }
}