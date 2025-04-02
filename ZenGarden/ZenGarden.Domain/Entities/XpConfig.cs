namespace ZenGarden.Domain.Entities;

public class XpConfig
{
    public int XpConfigId { get; set; }
    public int FocusMethodId { get; set; }
    public int TaskTypeId { get; set; }
    public double BaseXp { get; set; }
    public double XpMultiplier { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public FocusMethod? FocusMethod { get; set; }
    public TaskType? TaskType { get; set; }
}