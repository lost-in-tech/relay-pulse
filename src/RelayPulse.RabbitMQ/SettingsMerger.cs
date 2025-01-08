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
            MessageTypeHeaderName =
                configSettings.MessageTypeHeaderName.TryPickNonEmpty(settings.MessageTypeHeaderName),
            MessageTypeValueConverter = configSettings.MessageTypeValueConverter.TryPickNonEmpty(settings.MessageTypeValueConverter),
            DefaultExpiryInSeconds = configSettings.DefaultExpiryInSeconds ?? settings.DefaultExpiryInSeconds,
            UseChannelPerType = configSettings.UseChannelPerType ?? settings.UseChannelPerType,
            Queues = configSettings.Queues ?? settings.Queues,

            DefaultPrefetchCount = configSettings.DefaultPrefetchCount ?? settings.DefaultPrefetchCount,

            AppIdHttpHeaderName = configSettings.AppIdHttpHeaderName ?? settings.AppIdHttpHeaderName ?? "x-app-id",
            TraceIdHttpHeaderName =
                configSettings.TraceIdHttpHeaderName ?? settings.TraceIdHttpHeaderName ?? "x-trace-id",
            TenantLogKey = configSettings.TenantLogKey ?? settings.TenantLogKey ?? "tenant",
            AppIdLogKey = configSettings.AppIdLogKey ?? settings.AppIdLogKey ?? "appId",
            ConsumerIdLogKey = configSettings.ConsumerIdLogKey ?? settings.ConsumerIdLogKey ?? "consumerId",
            TraceIdLogKey = configSettings.TraceIdLogKey ?? settings.TraceIdLogKey ?? "traceId",
            UserIdLogKey = configSettings.UserIdLogKey ?? settings.UserIdLogKey ?? "userId",
            SentAtHeaderName = configSettings.SentAtHeaderName ?? settings.SentAtHeaderName ?? Constants.HeaderSentAt,
            MessageIdLogKey = configSettings.MessageIdLogKey ?? settings.MessageIdLogKey ?? Constants.MessageIdLogKey,
            QueueLogKey = configSettings.QueueLogKey ?? settings.QueueLogKey ?? Constants.QueueLogKey
        };
    }
}