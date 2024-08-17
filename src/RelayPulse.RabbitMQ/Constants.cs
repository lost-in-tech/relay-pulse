namespace RelayPulse.RabbitMQ;

internal static class Constants
{
    public const string HeaderExchange = "_relayhub_exchange_name";
    public const string HeaderRoutingKey = "_relayhub_routing_key";
    public const string HeaderExpiryKey = "_relayhub_expiry";

    public const string HeaderMsgTypeFull = "rp-msg-type-full";
    public const string HeaderMsgTypeShort = "rp-msg-type";
    public const string HeaderTenant = "rp-tenant";
    public const string HeaderAppId = "rp-appid";
}