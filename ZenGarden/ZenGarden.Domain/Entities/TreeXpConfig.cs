namespace ZenGarden.Domain.Entities;

public class TreeXpConfig
{
    public int LevelId { get; set; }
    public int XpThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<UserTree> UserTrees { get; set; } = new List<UserTree>();
}