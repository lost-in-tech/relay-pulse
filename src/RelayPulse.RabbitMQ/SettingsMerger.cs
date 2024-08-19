namespace RelayPulse.RabbitMQ;

internal static class SettingsMerger
{
    public static RabbitMqSettings Read(RabbitMqSettings? settings, RabbitMqSettings configSettings)
    {
        if (settings == null) return configSettings;
        
        return new RabbitMqSettings
        {
            Uri = configSettings.Uri.TryPickNonEmpty(settings.Uri).NullToEmpty(),
            DefaultExchange = configSettings.DefaultExchange.TryPickNonEmpty(settings.DefaultExchange).NullToEmpty(),
            DefaultExchangeType = configSettings.DefaultExchangeType.TryPickNonEmpty(settings.DefaultExchangeType),
            TypePrefix = configSettings.TypePrefix.TryPickNonEmpty(settings.TypePrefix),
            AppId = configSettings.AppId.TryPickNonEmpty(settings.AppId),
            DefaultTenant = configSettings.DefaultTenant.TryPickNonEmpty(settings.DefaultTenant),
            TenantHeaderName = configSettings.TenantHeaderName.TryPickNonEmpty(settings.TenantHeaderName),
            AppIdHeaderName = configSettings.AppIdHeaderName.TryPickNonEmpty(settings.AppIdHeaderName),
            MessageTypeHeaderName =  configSettings.MessageTypeHeaderName.TryPickNonEmpty(settings.MessageTypeHeaderName),
            DefaultExpiryInSeconds = configSettings.DefaultExpiryInSeconds ?? settings.DefaultExpiryInSeconds,
            UseChannelPerType = configSettings.UseChannelPerType ?? settings.UseChannelPerType,
            Queues = configSettings.Queues ?? settings.Queues
        };
    }
}