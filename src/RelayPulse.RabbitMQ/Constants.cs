namespace RelayPulse.RabbitMQ;

internal static class Constants
{
    public const string HeaderExchange = "_relayhub_exchange_name";
    public const string HeaderRoutingKey = "_relayhub_routing_key";
    public const string HeaderExpiryKey = "_relayhub_expiry";

    public const string HeaderMsgType = "rp-msg-type";
}