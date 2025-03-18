namespace ZenGarden.Domain.DTOs;

public class UpdateTaskTypeDto
{
    public string TaskTypeName { get; set; } = string.Empty;
    public string? TaskTypeDescription { get; set; }
}