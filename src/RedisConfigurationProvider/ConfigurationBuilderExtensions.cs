using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisConfigurationProvider;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///     Adds the Redis configuration provider with DbContextOptionsBuilder to builder.
    /// </summary>
    /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.</param>
    /// <param name="applicationName">Application name where configuration binds</param>
    /// <param name="connectionString">Redis connection string where configuration binds </param>
    /// <param name="refreshTimerIntervalInMs">Cache invalidation time in milliseconds</param>
    /// <param name="onLoadException">Error callback for exception when reload settings</param>
    /// <returns>The Microsoft.Extensions.Configuration.IConfigurationBuilder.</returns>
    public static IConfigurationBuilder AddRedisConfiguration(
        this IConfigurationBuilder builder,
        string applicationName,
        string connectionString,
        int refreshTimerIntervalInMs,
        Action<ConfigurationRedisLoadExceptionContext>? onLoadException = null) =>
        builder.Add(new RedisConfigurationSource(applicationName, connectionString, refreshTimerIntervalInMs, onLoadException));
}