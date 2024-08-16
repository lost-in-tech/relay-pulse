using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IChannelFactory : IDisposable
{
    IModel GetOrCreate(string typeName);
    bool IsApplicable(string typeName, bool forPublisher);
}

internal sealed class PublisherDefaultChannelFactory(
    IPublisherChannelSettings settings,
    IRabbitMqConnectionInstance connection) 
    : IChannelFactory
{
    private readonly Lazy<IModel> _lazyChannel = new(() => connection.Get().CreateModel());

    public IModel GetOrCreate(string typeName) => _lazyChannel.Value;

    public bool IsApplicable(string typeName, bool forPublisher)
    {
        return forPublisher && settings.UseChannelPerType is null or false;
    }
    
    public void Dispose()
    {
        if (_lazyChannel is { IsValueCreated: true, Value.IsClosed: false })
        {
            _lazyChannel.Value.Close();
        }
    }

}

internal sealed class PublisherPerTypeChannelFactory(
    IRabbitMqConnectionInstance connectionInstance,
    IPublisherChannelSettings settings) 
    : IChannelFactory
{
    private readonly ConcurrentDictionary<string,Lazy<IModel>> _source = new();
    
    public IModel GetOrCreate(string key)
    {
        var lazyChannel = _source.GetOrAdd(key, _ => new Lazy<IModel>(() =>  connectionInstance.Get().CreateModel()));

        var channel = lazyChannel.Value;
        
        if (channel.IsClosed)
        {
            var newChannel = _source.AddOrUpdate(key, 
                _ => new Lazy<IModel>(() => connectionInstance.Get().CreateModel()),
                (_, cnl) => cnl.Value.IsClosed ? new Lazy<IModel>(() => connectionInstance.Get().CreateModel()) : cnl);

            return newChannel.Value;
        }
        
        return channel;
    }

    public bool IsApplicable(string typeName, bool forPublisher)
    {
        return forPublisher && (settings.UseChannelPerType ?? false);
    }

    public void Dispose()
    {
        foreach (var lazyChannel in _source)
        {
            var channel = lazyChannel.Value;
            
            if (!channel.Value.IsClosed)
            {
                channel.Value.Close();
                channel.Value.Dispose();
            }
        }
    }
}

public interface IPublisherChannelSettings
{
    public bool? UseChannelPerType { get; }
}