using System.Text;
using Bolt.Common.Extensions;
using NSubstitute;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqConnectionInstance : IRabbitMqConnectionInstance
{
    public IConnection Get()
    {
        var fake = Substitute.For<IConnection>();
        var fakeModel = Substitute.For<IModel>();
        fakeModel.CreateBasicProperties().Returns(new FakeBasicProp());
        fake.CreateModel().Returns(fakeModel);
        
        return fake;
    }
    
    public class FakeBasicProp : IBasicProperties
    {
        public ushort ProtocolClassId { get; set; } 
        public string ProtocolClassName { get; set; } = string.Empty;
        public void ClearAppId(){}

        public void ClearClusterId(){}

        public void ClearContentEncoding(){}

        public void ClearContentType(){}

        public void ClearCorrelationId(){}

        public void ClearDeliveryMode(){}

        public void ClearExpiration(){}

        public void ClearHeaders(){}

        public void ClearMessageId(){}

        public void ClearPriority(){}

        public void ClearReplyTo(){}

        public void ClearTimestamp(){}

        public void ClearType(){}

        public void ClearUserId(){}

        public bool IsAppIdPresent() => AppId.HasValue();

        public bool IsClusterIdPresent() => false;

        public bool IsContentEncodingPresent() => ContentEncoding.HasValue();

        public bool IsContentTypePresent() => ContentType.HasValue();

        public bool IsCorrelationIdPresent() => CorrelationId.HasValue();

        public bool IsDeliveryModePresent() => false;

        public bool IsExpirationPresent() => false;

        public bool IsHeadersPresent() => Headers.Count > 0;

        public bool IsMessageIdPresent() => MessageId.HasValue();

        public bool IsPriorityPresent() => Priority is > 0 and <= 9;

        public bool IsReplyToPresent() => false;

        public bool IsTimestampPresent() => false;

        public bool IsTypePresent() => Type.HasValue();

        public bool IsUserIdPresent() => UserId.HasValue();

        public string? AppId { get; set; }
        public string? ClusterId { get; set; }
        public string? ContentEncoding { get; set; }
        public string? ContentType { get; set; }
        public string? CorrelationId { get; set; }
        public byte DeliveryMode { get; set; }
        public string? Expiration { get; set; }
        public IDictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        public string? MessageId { get; set; }
        public bool Persistent { get; set; }
        public byte Priority { get; set; }
        public string? ReplyTo { get; set; }
        public PublicationAddress? ReplyToAddress { get; set; }
        public AmqpTimestamp Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}