﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class RedisService : IRedisService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IDatabase _database;
    private readonly ILogger<RedisService> _logger;
    private readonly ConnectionMultiplexer _redis;

    public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        try
        {
            // Lấy thông tin kết nối từ appsettings.json
            var host = _configuration["Redis:Host"];
            var portStr = _configuration["Redis:Port"];
            var password = _configuration["Redis:Password"];
            var user = _configuration["Redis:User"] ?? "default";
            var useSSL = bool.Parse(_configuration["Redis:UseSSL"] ?? "false");

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
                ConnectTimeout = int.Parse(_configuration["Redis:ConnectTimeout"] ?? "15000"),
                SyncTimeout = int.Parse(_configuration["Redis:SyncTimeout"] ?? "10000"),
                Ssl = useSSL,
                ClientName = _configuration["Redis:ClientName"] ?? "ZenGardenApp"
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
        return (await _database.StringGetAsync(key))!;
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

    public void Dispose()
    {
        _redis?.Dispose();
    }

    public async Task<TimeSpan?> GetKeyTimeToLiveAsync(string key)
    {
        return await _database.KeyTimeToLiveAsync(key);
    }
}