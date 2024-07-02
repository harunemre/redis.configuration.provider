using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisConfigurationProvider;

public class RedisConfigurationProvider : ConfigurationProvider, IDisposable
{
    private static readonly string RedisConfigurationKeyPrefix = "Redis_Configuration_";
    private static readonly string RedisSubscriptionChannelPrefix = "Redis_Configuration_Subscription_";
    private readonly RedisConfigurationSource _configurationSource;
    private bool _disposed;
    private readonly string _redisConfigurationKey;
    private readonly string _redisSubscriptionKey;
    private bool _isSubscribed;
    private readonly Timer? _timer;

    public RedisConfigurationProvider(RedisConfigurationSource configurationSource)
    {
        _configurationSource = configurationSource;

        _redisConfigurationKey = $"{RedisConfigurationKeyPrefix}:{_configurationSource.ApplicationName}";
        _redisSubscriptionKey = $"{RedisSubscriptionChannelPrefix}:{_configurationSource.ApplicationName}";
        _timer = new Timer
            (
                ReloadSettings,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(_configurationSource.RefreshTimerIntervalInMs)
            );
    }

    private void ReloadSettings(object? state)
    {
        Load();
        OnReload();
    }

    public override void Load()
    {
        try
        {
            var isKeyExist = RedisConnection
            .GetInstance(_configurationSource.ConfigurationOptions)
            .Connection?
            .GetDatabase()
            .KeyExists(_redisConfigurationKey);

            if (isKeyExist == false)
            {
                List<SettingEntity> settings = [
                    new SettingEntity(1, "SiteName", "String", "Boyner.com.tr", 1, "SERVICE-A"),
                    new SettingEntity(2, "IsBasketEnabled", "Boolean", 1, 1, "SERVICE-B"),
                    new SettingEntity(3, "MaxItemCount", "Int", 50, 0, "SERVICE-A")
                ];

                foreach (var setting in settings)
                {
                    var hashEntries = new HashEntry[]
                    {
                        new(nameof(setting.ID), setting.ID),
                        new(nameof(setting.Name), setting.Name),
                        new(nameof(setting.Type), setting.Type),
                        new(nameof(setting.Value), setting.Value.ToString()),
                        new(nameof(setting.IsActive), setting.IsActive),
                        new(nameof(setting.ApplicationName), setting.ApplicationName)
                    };

                    RedisConnection
                   .GetInstance(_configurationSource.ConfigurationOptions)
                   .Connection?
                   .GetDatabase()
                   .HashSet($"{_redisConfigurationKey}:{setting.Name}", hashEntries);
                }
            }

            Data = RedisConnection
            .GetInstance(_configurationSource.ConfigurationOptions)
            .Connection?
            .GetDatabase()
            .HashGetAll(_redisConfigurationKey)
            .ToStringDictionary();

        }
        catch (Exception ex)
        {
            var exceptionContext = new ConfigurationRedisLoadExceptionContext(_configurationSource, ex);
            _configurationSource.OnLoadException?.Invoke(exceptionContext);
            if (!exceptionContext.Ignore)
            {
                throw;
            }
        }

        if (!_isSubscribed)
        {
            SubscribeRedis();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        RedisConnection.GetInstance(_configurationSource.ConfigurationOptions).Connection?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void SubscribeRedis()
    {
        try
        {
            ISubscriber? subscriber = RedisConnection.GetInstance(_configurationSource.ConfigurationOptions).Connection?.GetSubscriber();

            if (subscriber is null)
            {
                return;
            }

            subscriber.Subscribe(new RedisChannel(_redisSubscriptionKey, RedisChannel.PatternMode.Auto), (channel, message) =>
            {
                Dictionary<string, string?>? newData = JsonSerializer.Deserialize<Dictionary<string, string?>>(message.ToString());
                if (newData is null)
                {
                    return;
                }

                Data = newData;
            });

            _isSubscribed = true;

        }
        catch (Exception ex)
        {
            var exceptionContext = new ConfigurationRedisLoadExceptionContext(_configurationSource, ex);
            _configurationSource.OnLoadException?.Invoke(exceptionContext);
            if (!exceptionContext.Ignore)
            {
                throw;
            }
        }
    }
}