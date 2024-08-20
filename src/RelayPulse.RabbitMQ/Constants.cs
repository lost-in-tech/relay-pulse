namespace RelayPulse.RabbitMQ;

internal static class Constants
{
    public const string HeaderExchange = "_relayhub_exchange_name";
    public const string HeaderRoutingKey = "_relayhub_routing_key";
    public const string HeaderExpiryKey = "_relayhub_expiry";
    
    public const string HeaderRetryCount = "rp-retry-count";

    public const string HeaderMsgType = "rp-msg-type";
    public const string HeaderTenant = "rp-tenant";
    public const string HeaderAppId = "rp-appid";
    
    public const string HeaderDeadLetterExchange = "x-dead-letter-exchange";
    public const string HeaderTimeToLive = "x-message-ttl";

    public const string ExchangeTypeHeader = "headers";
    public const string ExchangeTypeFanout = "fanout";
    public const string ExchangeTypeDirect = "direct";
    public const string ExchangeTypeTopic = "topic";
    public const string HeaderTargetQueue = "rp-target-queue";
    
    public const string HeaderMatch = "x-match";
    public const string HeaderSentAt = "rp-sent-at";
    

    public static string RouteKeyTargetQueue(string queue) => $"rp:target:{queue}";
}