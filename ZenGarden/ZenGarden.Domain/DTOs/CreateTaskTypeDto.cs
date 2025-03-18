namespace ZenGarden.Domain.DTOs;

public class CreateTaskTypeDto
{
    public string TaskTypeName { get; set; } = string.Empty;
    public string? TaskTypeDescription { get; set; }
}