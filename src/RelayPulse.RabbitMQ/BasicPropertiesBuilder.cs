using RabbitMQ.Client;
using RelayPulse.Core;

namespace RelayPulse.RabbitMQ;

internal sealed class BasicPropertiesBuilder(IMessagePublishSettings settings)
{
    public IBasicProperties Build<T>(Guid id, IModel channel, Message<T> msg)
    {
        var prop = channel.CreateBasicProperties();
        prop.Headers = new Dictionary<string, object>();

        var msgHeaders = msg.Headers;
        
        var appName = msg.AppId.TryPickNonEmpty(settings.AppId);

        if (appName.HasValue())
        {
            prop.AppId = appName;
            prop.Headers[Constants.HeaderAppId] = appName;
        }
        
        var expiryValue = msgHeaders.PopAsDouble(Constants.HeaderExpiryKey) ?? settings.DefaultExpiryInSeconds;
        if (expiryValue is > 0)
        {
            prop.Expiration = (expiryValue.Value * 1000).ToString("F0");
        }
        
        var tenant = msg.Tenant.TryPickNonEmpty(settings.DefaultTenant);
        if (tenant.HasValue())
        {
            prop.Headers[Constants.HeaderTenant] = tenant;
        }

        var type = typeof(T);
        var typeFullName = msg.Type.EmptyAlternative(type.FullName.EmptyAlternative(type.Name));
        prop.Type = typeFullName;

        var typeName = $"{settings.TypePrefix}{msg.Type.EmptyAlternative(type.Name.ToSnakeCase())}";
        prop.Headers[settings.MessageTypeHeaderName.EmptyAlternative(Constants.HeaderMsgType)] = typeName;


        if (msg.Cid.HasValue())
        {
            prop.CorrelationId = msg.Cid;
        }

        prop.ContentEncoding = "utf-8";
        prop.ContentType = "application/json";

        prop.MessageId = id.ToString();
        if (msg.UserId.HasValue())
        {
            prop.UserId = msg.UserId;
        }

        foreach (var header in msgHeaders)
        {
            prop.Headers[header.Key] = header.Value;
        }

        return prop;
    }

}