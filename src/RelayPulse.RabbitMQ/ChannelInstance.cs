using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace RelayPulse.RabbitMQ;

internal interface IChannelInstance : IDisposable
{
    IModel GetOrCreate(string key);
}

internal sealed class ChannelInstance(IRabbitMqConnectionInstance connectionInstance) : IChannelInstance
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