namespace ZenGarden.Domain.Entities;

public class TreeXpConfig
{
    public int Level { get; set; }
    public int XpThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}