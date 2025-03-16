namespace ZenGarden.Domain.DTOs;

public class TaskTypeDto
{
    public int TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string? TaskTypeDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TaskDto> Tasks { get; set; } = new List<TaskDto>();
    public ICollection<XpConfigDto> XpConfigs { get; set; } = new List<XpConfigDto>();
}