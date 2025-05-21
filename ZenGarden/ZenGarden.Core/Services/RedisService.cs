using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class RedisService : IRedisService, IDisposable
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisService> _logger;
    private readonly ConnectionMultiplexer _redis;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
    {
        _logger = logger;

        try
        {
            // Lấy thông tin kết nối từ appsettings.json
            var host = configuration["Redis:Host"];
            var portStr = configuration["Redis:Port"];
            var password = configuration["Redis:Password"];
            var user = configuration["Redis:User"] ?? "default";
            var useSsl = bool.Parse(configuration["Redis:UseSSL"] ?? "true");

            // Kiểm tra thông tin cấu hình
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Redis configuration is missing or incomplete in appsettings.json");

            // Parse port
            if (!int.TryParse(portStr, out var port)) throw new ArgumentException("Invalid Redis port configuration");

            // Tạo cấu hình kết nối
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                Password = password,
                User = user,
                AbortOnConnectFail = false,
                ConnectTimeout = int.Parse(configuration["Redis:ConnectTimeout"] ?? "30000"),
                SyncTimeout = int.Parse(configuration["Redis:SyncTimeout"] ?? "20000"),
                Ssl = useSsl,
                ClientName = configuration["Redis:ClientName"] ?? "ZenGardenApp",
                ReconnectRetryPolicy = new LinearRetry(RetryDelayMs),
                ConnectRetry = MaxRetries,
                KeepAlive = 60
            };

            _logger.LogInformation("Attempting to connect to Redis at {host}:{port}", host, port);
            _redis = ConnectionMultiplexer.Connect(configOptions);
            _database = _redis.GetDatabase();

            _logger.LogInformation("Connected to Redis server successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while connecting to the Redis server.");
            throw;
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        var retries = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (RedisConnectionException ex) when (retries < MaxRetries)
            {
                retries++;
                _logger.LogWarning(ex, "Redis connection error during {Operation}. Retry {Retry} of {MaxRetries}", 
                    operationName, retries, MaxRetries);
                await Task.Delay(RetryDelayMs * retries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Redis operation {Operation}", operationName);
                throw;
            }
        }
    }

    public async Task<bool> PingAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var result = await _database.PingAsync();
            _logger.LogInformation("Redis ping successful: {Result}", result);
            return result > TimeSpan.Zero;
        }, "Ping");
    }

    public async Task<string> GetStringAsync(string key)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var result = await _database.StringGetAsync(key);
            return result.IsNullOrEmpty ? string.Empty : result.ToString();
        }, "GetString");
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _database.StringSetAsync(key, value, expiry);
        }, "SetString");
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<TimeSpan?> GetKeyTimeToLiveAsync(string key)
    {
        return await _database.KeyTimeToLiveAsync(key);
    }

    public async Task<bool> AddToSortedSetAsync(string setKey, string member, double score)
    {
        return await _database.SortedSetAddAsync(setKey, member, score);
    }

    public async Task<List<string>> GetTopSortedSetAsync(string setKey, int count)
    {
        var result = await _database.SortedSetRangeByRankAsync(setKey, 0, count - 1, Order.Descending);
        return result.Select(x => x.ToString()).ToList();
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getDataFunc, TimeSpan? expiry = null)
    {
        var cached = await _database.StringGetAsync(key);
        if (!cached.IsNullOrEmpty)
        {
            var cachedString = cached.ToString();
            return JsonSerializer.Deserialize<T>(cachedString)
                   ?? throw new InvalidOperationException("Failed to deserialize cached data");
        }

        var data = await getDataFunc();
        var json = JsonSerializer.Serialize(data);
        await _database.StringSetAsync(key, json, expiry);
        return data;
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        return await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var redisValue = await _database.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty) return default;
            var jsonString = redisValue.ToString();
            return string.IsNullOrEmpty(jsonString) ? default : JsonSerializer.Deserialize<T>(jsonString);
        }, "Get");
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public void Dispose()
    {
        _redis.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        var keys = server.Keys(pattern: pattern);

        foreach (var key in keys) await _redis.GetDatabase().KeyDeleteAsync(key);
    }
}