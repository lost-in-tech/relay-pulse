using NSubstitute;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ.Tests.Fakes;

public class FakeRabbitMqConnectionInstance : IRabbitMqConnectionInstance
{
    public IConnection Get()
    {
        var fake = Substitute.For<IConnection>();
        fake.CreateModel().Returns(Substitute.For<IModel>());
        return fake;
    }
}