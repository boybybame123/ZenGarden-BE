// Controllers/RedisController.cs

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly ILogger<RedisController> _logger;
    private readonly IRedisService _redisService;

    public RedisController(
        IRedisService redisService,
        ILogger<RedisController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            var isAlive = await _redisService.PingAsync();
            return Ok(new
            {
                success = true,
                message = "Redis is alive",
                ping = isAlive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinging Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to ping Redis"
            });
        }
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get([Required] string key)
    {
        try
        {
            var value = await _redisService.GetStringAsync(key);

            if (value == null)
                return NotFound(new
                {
                    success = false,
                    message = "Key not found"
                });

            return Ok(new
            {
                success = true,
                key,
                value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting key {key} from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to get value from Redis"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Set(
        [FromBody] SetRedisValueRequest request)
    {
        try
        {
            var result = request is { Key: not null, Value: not null } && await _redisService.SetStringAsync(
                request.Key,
                request.Value,
                request.ExpiryInSeconds.HasValue
                    ? TimeSpan.FromSeconds(request.ExpiryInSeconds.Value)
                    : null);

            if (!result)
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to set value in Redis"
                });

            return Ok(new
            {
                success = true,
                message = "Value set successfully",
                key = request.Key
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting key {request.Key} in Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to set value in Redis"
            });
        }
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete([Required] string key)
    {
        try
        {
            var result = await _redisService.DeleteKeyAsync(key);

            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Key not found or already deleted"
                });

            return Ok(new
            {
                success = true,
                message = "Key deleted successfully",
                key
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting key {key} from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to delete key from Redis"
            });
        }
    }

    [HttpGet("exists/{key}")]
    public async Task<IActionResult> Exists([Required] string key)
    {
        try
        {
            var exists = await _redisService.KeyExistsAsync(key);
            return Ok(new
            {
                success = true,
                key,
                exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking existence of key {key} in Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to check key existence in Redis"
            });
        }
    }
}

public class SetRedisValueRequest
{
    [Required] public string? Key { get; set; }

    [Required] public string? Value { get; set; }

    public int? ExpiryInSeconds { get; set; }
}