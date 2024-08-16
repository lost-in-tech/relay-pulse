namespace RelayPulse.Core.Fluent;

public interface IHaveMessage :
    ICollectTypeName,
    ICollectTenant,
    ICollectHeaders,
    ICollectUserId,
    ICollectAppId,
    ICollectCid,
    IPublishMessage
{
}

public interface ICollectTypeName
{
    IHaveTypeName Type(string typeName);
}

public interface IHaveTypeName :
    ICollectTenant,
    ICollectHeaders,
    ICollectUserId,
    ICollectAppId,
    ICollectCid,
    IPublishMessage
{
}

public interface ICollectTenant
{
    IHaveTenant Tenant(string tenant);
}

public interface IHaveTenant :
    ICollectHeaders,
    ICollectUserId,
    ICollectAppId,
    ICollectCid,
    IPublishMessage
{}

public interface ICollectHeaders
{
    IHaveHeaders Header(string name, string? value);
    IHaveHeaders Headers(Dictionary<string, string?> value);
}

public interface IHaveHeaders : 
    ICollectHeaders,
    ICollectUserId,
    ICollectAppId,
    ICollectCid,
    IPublishMessage
{
}

public interface ICollectUserId
{
    IHaveUserId UserId(string userId);
}

public interface IHaveUserId :
    ICollectAppId,
    ICollectCid,
    IPublishMessage
{
}

public interface ICollectAppId
{
    IHaveAppId AppId(string appId);
}

public interface IHaveAppId : ICollectCid, IPublishMessage
{
}


public interface ICollectCid
{
    IHaveCid Cid(string cid);
}

public interface IHaveCid : IPublishMessage
{
}

public interface IPublishMessage
{
    Task<bool> Publish(CancellationToken ct = default);
}

internal class FluentMessagePublisher<T>(IMessagePublisher publisher, T msg, Guid? id) :
    IHaveTypeName,
    IHaveTenant,
    IHaveHeaders,
    IHaveAppId,
    IHaveUserId,
    IHaveCid,
    IHaveMessage
{
    private string? _appId;
    private string? _type;
    private string? _cid;
    private string? _userId;
    private string? _tenant;
    private Dictionary<string, string> _headers = new();

    public Task<bool> Publish(CancellationToken ct = default)
    {
        return publisher.Publish(new Message<T>
        {
            Content = msg,
            UserId = _userId,
            Headers = _headers,
            AppId = _appId,
            Cid = _cid,
            Type = _type,
            Id = id,
            Tenant = _tenant
        }, ct);
    }

    public IHaveAppId AppId(string appId)
    {
        _appId = appId;

        return this;
    }

    public IHaveTypeName Type(string typeName)
    {
        _type = typeName;
        return this;
    }

    public IHaveUserId UserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public IHaveHeaders Header(string name, string? value)
    {
        if (value == null) return this;
        _headers[name] = value;
        return this;
    }

    public IHaveHeaders Headers(Dictionary<string, string?> headers)
    {
        if (headers.Count == 0) return this;
        foreach (var kv in headers)
        {
            if(kv.Value == null) continue;
            
            _headers[kv.Key] = kv.Value;
        }

        return this;
    }

    public IHaveCid Cid(string cid)
    {
        _cid = cid;
        return this;
    }

    public IHaveTenant Tenant(string tenant)
    {
        _tenant = tenant;
        return this;
    }
}
