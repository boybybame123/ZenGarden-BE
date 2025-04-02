namespace ZenGarden.Domain.DTOs;

public class TaskTypeDto
{
    public int TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string? TaskTypeDescription { get; set; }
    public double XpMultiplier { get; set; }
}