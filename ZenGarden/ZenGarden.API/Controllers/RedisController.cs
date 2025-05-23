// Controllers/RedisController.cs

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController(
    IRedisService redisService,
    ILogger<RedisController> logger)
    : ControllerBase
{
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            var isAlive = await redisService.PingAsync();
            return Ok(new
            {
                success = true,
                message = "Redis is alive",
                ping = isAlive
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error pinging Redis");
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
            var value = await redisService.GetStringAsync(key);
            return Ok(new
            {
                success = true,
                key,
                value
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error getting key {key} from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to get value from Redis"
            });
        }
    }

    [HttpPost("set")]
    public async Task<IActionResult> Set([FromBody] SetRedisValueRequest request)
    {
        try
        {
            TimeSpan? expiry = null;
            if (request.ExpiryInSeconds.HasValue)
            {
                expiry = TimeSpan.FromSeconds(request.ExpiryInSeconds.Value);
            }

            var result = await redisService.SetStringAsync(request.Key!, request.Value!, expiry);
            return Ok(new
            {
                success = true,
                key = request.Key,
                value = request.Value,
                expiry = request.ExpiryInSeconds,
                result
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting key {RequestKey} in Redis", request.Key);
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
            var result = await redisService.DeleteKeyAsync(key);
            return Ok(new
            {
                success = true,
                key,
                deleted = result
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error deleting key {key} from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to delete key from Redis"
            });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> SetMultiple([FromBody] Dictionary<string, string> keyValuePairs)
    {
        try
        {
            var result = await redisService.SetMultipleAsync(keyValuePairs);
            return Ok(new
            {
                success = true,
                result
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting multiple keys in Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to set multiple values in Redis"
            });
        }
    }

    [HttpGet("batch")]
    public async Task<IActionResult> GetMultiple([FromQuery] string[] keys)
    {
        try
        {
            var result = await redisService.GetMultipleAsync<string>(keys);
            return Ok(new
            {
                success = true,
                values = result
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting multiple keys from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to get multiple values from Redis"
            });
        }
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var metrics = await redisService.GetMetricsAsync();
            return Ok(new
            {
                success = true,
                metrics
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Redis metrics");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to get Redis metrics"
            });
        }
    }

    [HttpDelete("pattern/{pattern}")]
    public async Task<IActionResult> DeleteByPattern([Required] string pattern)
    {
        try
        {
            await redisService.RemoveByPatternAsync(pattern);
            return Ok(new
            {
                success = true,
                pattern,
                message = "Keys matching pattern have been deleted"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error deleting keys matching pattern {pattern} from Redis");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to delete keys by pattern from Redis"
            });
        }
    }

    [HttpPost("lock/{key}")]
    public async Task<IActionResult> AcquireLock([Required] string key, [FromQuery] int expirySeconds = 30)
    {
        try
        {
            var result = await redisService.AcquireLockAsync(key, TimeSpan.FromSeconds(expirySeconds));
            return Ok(new
            {
                success = true,
                key,
                acquired = result,
                expirySeconds
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error acquiring lock for key {key}");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to acquire lock"
            });
        }
    }

    [HttpDelete("lock/{key}")]
    public async Task<IActionResult> ReleaseLock([Required] string key)
    {
        try
        {
            await redisService.ReleaseLockAsync(key);
            return Ok(new
            {
                success = true,
                key,
                message = "Lock released successfully"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error releasing lock for key {key}");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to release lock"
            });
        }
    }

    [HttpPost("publish/{channel}")]
    public async Task<IActionResult> Publish([Required] string channel, [FromBody] string message)
    {
        try
        {
            await redisService.PublishAsync(channel, message);
            return Ok(new
            {
                success = true,
                channel,
                message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error publishing message to channel {channel}");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to publish message"
            });
        }
    }
}