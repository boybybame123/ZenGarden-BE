using System.ComponentModel.DataAnnotations;

namespace ZenGarden.Domain.DTOs;

public class SetRedisValueRequest
{
    [Required] public string? Key { get; set; }

    [Required] public string? Value { get; set; }

    public int? ExpiryInSeconds { get; set; }
}