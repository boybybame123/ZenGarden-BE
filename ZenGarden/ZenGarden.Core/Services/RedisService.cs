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
    private readonly TimeSpan _defaultExpiry;
    private readonly CircuitBreaker _circuitBreaker;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;
    private readonly Dictionary<string, TimeSpan> _defaultExpiryByType;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
    {
        _logger = logger;
        _defaultExpiry = TimeSpan.FromMinutes(30);
        var instanceName = configuration["Redis:ClientName"] ?? "ZenGardenApp";
        _circuitBreaker = new CircuitBreaker(5, TimeSpan.FromSeconds(30));
        
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };
        
        _defaultExpiryByType = new Dictionary<string, TimeSpan>
        {
            { "user", TimeSpan.FromHours(1) },
            { "session", TimeSpan.FromMinutes(30) },
            { "cache", TimeSpan.FromMinutes(15) }
        };

        try
        {
            var host = configuration["Redis:Host"];
            var portStr = configuration["Redis:Port"];
            var password = configuration["Redis:Password"];
            var user = configuration["Redis:User"] ?? "default";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Redis configuration is missing or incomplete in appsettings.json");

            if (!int.TryParse(portStr, out var port)) 
                throw new ArgumentException("Invalid Redis port configuration");

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                Password = password,
                User = user,
                AbortOnConnectFail = false,
                ConnectTimeout = 10000, // Increased timeout
                SyncTimeout = 10000,    // Increased timeout
                Ssl = false,            // Disabled SSL
                ClientName = instanceName,
                ReconnectRetryPolicy = new LinearRetry(RetryDelayMs),
                ConnectRetry = MaxRetries,
                KeepAlive = 60,
                AllowAdmin = true,
                TieBreaker = "",
                ConfigCheckSeconds = 60
            };

            _logger.LogInformation("Attempting to connect to Redis at {host}:{port}", host, port);
            
            try
            {
                _redis = ConnectionMultiplexer.Connect(configOptions);
                _database = _redis.GetDatabase();

                // Test connection with retry
                var retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        var pingResult = _database.Ping();
                        if (pingResult.TotalMilliseconds > 0)
                        {
                            _logger.LogInformation("Connected to Redis server successfully. Instance: {InstanceName}", instanceName);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount == 3)
                            throw;
                        
                        _logger.LogWarning(ex, "Redis ping attempt {RetryCount} failed, retrying...", retryCount);
                        Thread.Sleep(1000 * retryCount);
                    }
                }
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Failed to connect to Redis server. Please check your connection settings.");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while configuring Redis connection");
            throw;
        }
    }

    private string GetKeyWithPrefix(string key, string prefix)
    {
        return $"{prefix}:{key}";
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        var retries = 0;
        var lastException = default(Exception);
        var startTime = DateTime.UtcNow;

        while (retries < MaxRetries)
        {
            try
            {
                var result = await _circuitBreaker.ExecuteAsync(operation);
                LogRedisOperation(operationName, startTime);
                return result;
            }
            catch (RedisConnectionException ex)
            {
                lastException = ex;
                retries++;
                _logger.LogWarning(ex, 
                    "Redis connection error during {Operation}. Retry {Retry} of {MaxRetries}", 
                    operationName, retries, MaxRetries);
                await Task.Delay(RetryDelayMs * retries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Redis operation {Operation}", operationName);
                throw;
            }
        }

        throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, 
            $"Failed to execute {operationName} after {MaxRetries} retries", lastException);
    }

    private void LogRedisOperation(string operation, DateTime startTime)
    {
        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            "Redis {Operation} completed in {Duration}ms",
            operation,
            duration.TotalMilliseconds
        );
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan? expiry)
    {
        return await ExecuteWithRetryAsync(async () => await _database.StringSetAsync(
            GetKeyWithPrefix(key, "lock"),
            Environment.MachineName,
            expiry,
            When.NotExists
        ), "AcquireLock");
    }

    public async Task ReleaseLockAsync(string key)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await _database.KeyDeleteAsync(GetKeyWithPrefix(key, "lock"));
            return true;
        }, "ReleaseLock");
    }

    [Obsolete("Obsolete")]
    public async Task PublishAsync(string channel, string message)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await _redis.GetSubscriber().PublishAsync(channel, message);
            return true;
        }, "Publish");
    }

    [Obsolete("Obsolete")]
    public void Subscribe(string channel, Action<string> handler)
    {
        _redis.GetSubscriber().Subscribe(channel, (_, value) => handler(value!));
    }

    public async Task<bool> PingAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var result = await _database.PingAsync();
            _logger.LogInformation("Redis ping successful: {Result}ms", result.TotalMilliseconds);
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
        return await ExecuteWithRetryAsync(async () => await _database.StringSetAsync(key, value, expiry ?? _defaultExpiry), "SetString");
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await ExecuteWithRetryAsync(async () => await _database.KeyExistsAsync(key), "KeyExists");
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await ExecuteWithRetryAsync(async () => await _database.KeyDeleteAsync(key), "DeleteKey");
    }

    public async Task<TimeSpan?> GetKeyTimeToLiveAsync(string key)
    {
        return await ExecuteWithRetryAsync(async () => await _database.KeyTimeToLiveAsync(key), "GetKeyTimeToLive");
    }

    public async Task<bool> AddToSortedSetAsync(string setKey, string member, double score)
    {
        return await ExecuteWithRetryAsync(async () => await _database.SortedSetAddAsync(setKey, member, score), "AddToSortedSet");
    }

    public async Task<List<string>> GetTopSortedSetAsync(string setKey, int count)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var result = await _database.SortedSetRangeByRankAsync(setKey, 0, count - 1, Order.Descending);
            return result.Select(x => x.ToString()).ToList();
        }, "GetTopSortedSet");
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getDataFunc, TimeSpan? expiry = null, string type = "cache")
    {
        try
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache miss for key: {Key}, fetching data...", key);
            var data = await getDataFunc();
            if (data != null)
            {
                var effectiveExpiry = expiry ?? _defaultExpiryByType.GetValueOrDefault(type, _defaultExpiry);
                await SetAsync(key, data, effectiveExpiry, type);
            }
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Redis operation GetOrSet");
            throw;
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, string type = "cache")
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var effectiveExpiry = expiry ?? _defaultExpiryByType.GetValueOrDefault(type, _defaultExpiry);
            return await _database.StringSetAsync(key, json, effectiveExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Redis operation Set");
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Redis operation Get");
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await _database.KeyDeleteAsync(key);
            return true;
        }, "Remove");
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogInformation("Removed {Count} keys matching pattern {Pattern}", keys.Length, pattern);
            }
            
            return true;
        }, "RemoveByPattern");
    }

    public async Task<bool> SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null, string type = "cache")
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var batch = _database.CreateBatch();
            var tasks = keyValuePairs.Select(kvp => 
                batch.StringSetAsync(
                    kvp.Key, 
                    JsonSerializer.Serialize(kvp.Value), 
                    expiry ?? _defaultExpiryByType.GetValueOrDefault(type, _defaultExpiry)
                ));
            
            await batch.ExecuteAsync("SET");
            var results = await Task.WhenAll(tasks);
            return results.All(x => x);
        }, "SetMultiple");
    }

    public async Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var enumerable = keys as string[] ?? keys.ToArray();
            var redisKeys = enumerable.Select(k => (RedisKey)k).ToArray();
            var values = await _database.StringGetAsync(redisKeys);
            
            var result = new Dictionary<string, T?>();
            for (var i = 0; i < enumerable.Length; i++)
            {
                var key = enumerable.ElementAt(i);
                var value = values[i];
                
                if (!value.IsNullOrEmpty)
                {
                    try
                    {
                        result[key] = JsonSerializer.Deserialize<T>(value.ToString());
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize data for key {Key}", key);
                        result[key] = default;
                    }
                }
                else
                {
                    result[key] = default;
                }
            }
            
            return result;
        }, "GetMultiple");
    }

    public async Task<RedisMetrics> GetMetricsAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var info = await server.InfoAsync();
            
            return new RedisMetrics
            {
                ConnectedClients = info.FirstOrDefault(x => x.Key == "connected_clients")?.ToString(),
                UsedMemory = info.FirstOrDefault(x => x.Key == "used_memory_human")?.ToString(),
                TotalConnectionsReceived = info.FirstOrDefault(x => x.Key == "total_connections_received")?.ToString(),
                TotalCommandsProcessed = info.FirstOrDefault(x => x.Key == "total_commands_processed")?.ToString(),
                UptimeInSeconds = info.FirstOrDefault(x => x.Key == "uptime_in_seconds")?.ToString()
            };
        }, "GetMetrics");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _redis.Close();
        _redis.Dispose();
    }
}