namespace RelayPulse.RabbitMQ;

internal static class Constants
{
    public const string HeaderExchange = "_relayhub_exchange_name";
    public const string HeaderRoutingKey = "_relayhub_routing_key";
    public const string HeaderExpiryKey = "_relayhub_expiry";
    
    public const string HeaderRetryCount = "rp-retry-count";

    public const string HeaderMsgType = "rp-msg-type";
    public const string HeaderTenant = "rp-tenant";
    public const string HeaderAppId = "rp-app-id";
    
    public const string HeaderDeadLetterExchange = "x-dead-letter-exchange";
    public const string HeaderDeadLetterRoutingKey = "x-dead-letter-routing-key";
    public const string HeaderTimeToLive = "x-message-ttl";
    
    public const string HeaderMatch = "x-match";
    public const string HeaderSentAt = "rp-sent-at";
    
    public const string TenantLogKey = "tenant";
    public const string TraceIdLogKey = "traceId";
    public const string ConsumerIdLogKey = "consumerId";
    public const string MessageIdLogKey = "msgId";
    public const string QueueLogKey = "queue";
    public const string UserIdLogKey = "userId";
    public const string AppIdLogKey = "appId";


    public static string RouteKeyTargetQueue(string queue) => $"rp:target:{queue}";
    
    public static string MessageTypeValueConverterSnakeCase = "SnakeCase";
}

public static class ExchangeTypesSupported
{
    public const string Headers = "headers";
    public const string Fanout = "fanout";
    public const string Direct = "direct";
    public const string Topic = "topic";
}