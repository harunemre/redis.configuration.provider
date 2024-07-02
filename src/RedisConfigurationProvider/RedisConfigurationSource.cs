using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisConfigurationProvider;

public class RedisConfigurationSource : IConfigurationSource
{
    
    public readonly string ApplicationName;
    public readonly ConfigurationOptions ConfigurationOptions;
    public readonly int RefreshTimerIntervalInMs;
    public readonly Action<ConfigurationRedisLoadExceptionContext>? OnLoadException;
    
    public RedisConfigurationSource(
        string applicationName,
        string connectionString,
        int refreshTimerIntervalInMs,
        Action<ConfigurationRedisLoadExceptionContext>? onLoadException
    )
    {
        ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
        ApplicationName = applicationName;
        RefreshTimerIntervalInMs = refreshTimerIntervalInMs;
        OnLoadException = onLoadException;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new RedisConfigurationProvider(this);
}