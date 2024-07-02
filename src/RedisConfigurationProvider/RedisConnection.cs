using StackExchange.Redis;

namespace RedisConfigurationProvider;

internal class RedisConnection
{
    private readonly Lazy<ConnectionMultiplexer>? LazyConnection;
    public ConnectionMultiplexer? Connection => LazyConnection?.Value;

    private static RedisConnection? Instance = null;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    private RedisConnection(ConfigurationOptions configurationOptions)
    {
        LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
    }

    public static RedisConnection GetInstance(ConfigurationOptions configurationOptions)
    {
        Semaphore.Wait();

        Instance ??= new RedisConnection(configurationOptions);

        Semaphore.Release();
        return Instance;
    }
}