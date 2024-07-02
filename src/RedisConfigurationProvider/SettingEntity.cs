namespace RedisConfigurationProvider;

public sealed record SettingEntity(
    int ID,
    string Name,
    string Type,
    object Value,
    int IsActive,
    string ApplicationName);

public sealed record SettingEntity<TValue>(int ID,string Name, TValue Value, bool IsActive,string ApplicationName);
