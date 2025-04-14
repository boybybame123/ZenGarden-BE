using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services
{
    public class RedisService : IRedisService, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            try
            {
                // Get connection details
                var host = "centerbeam.proxy.rlwy.net";
                var port = 20762;
                var password = "ErBpHLsdsDMxlZMpSZXqKqTmuiSUFfzp";

                // For Railway connections, try this format
                var configOptions = new ConfigurationOptions
                {
                    EndPoints = { { host, port } },
                    Password = password,
                    User = "default", // Railway often requires this user
                    AbortOnConnectFail = false,
                    ConnectTimeout = 15000,
                    SyncTimeout = 10000,
                    Ssl = false, // Try without SSL first
                    ClientName = "ZenGardenApp"
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

        public async Task<bool> PingAsync()
        {
            try
            {
                var result = await _database.PingAsync();
                _logger.LogInformation("Redis ping successful: {Result}", result);
                return result > TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while pinging the Redis server.");
                return false;
            }
        }

        public async Task<string> GetStringAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _database.StringSetAsync(key, value, expiry);
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

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
}