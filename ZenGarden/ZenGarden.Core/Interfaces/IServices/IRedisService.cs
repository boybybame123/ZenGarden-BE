namespace ZenGarden.Core.Interfaces.IServices;

public interface IRedisService
{
    Task<bool> PingAsync();
    Task<string> GetStringAsync(string key);
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
    Task<bool> DeleteKeyAsync(string key);
    Task<TimeSpan?> GetKeyTimeToLiveAsync(string key);
    Task<bool> AddToSortedSetAsync(string setKey, string member, double score);
    Task<List<string>> GetTopSortedSetAsync(string setKey, int count);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getDataFunc, TimeSpan? expiry = null, string type = "cache");
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, string type = "cache");
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null, string type = "cache");
    Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);
    Task<RedisMetrics> GetMetricsAsync();
    Task<bool> AcquireLockAsync(string key, TimeSpan? expiry);
    Task ReleaseLockAsync(string key);
    Task PublishAsync(string channel, string message);
    void Subscribe(string channel, Action<string> handler);
    void Dispose();
}

public class RedisMetrics
{
    public string? ConnectedClients { get; set; }
    public string? UsedMemory { get; set; }
    public string? TotalConnectionsReceived { get; set; }
    public string? TotalCommandsProcessed { get; set; }
    public string? UptimeInSeconds { get; set; }
}