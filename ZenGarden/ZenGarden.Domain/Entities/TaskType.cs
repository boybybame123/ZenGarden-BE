namespace ZenGarden.Domain.Entities;

public class TaskType
{
    public int TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string? TaskTypeDescription { get; set; }
    public double XpMultiplier { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
    public virtual ICollection<XpConfig> XpConfigs { get; set; } = new List<XpConfig>();
}