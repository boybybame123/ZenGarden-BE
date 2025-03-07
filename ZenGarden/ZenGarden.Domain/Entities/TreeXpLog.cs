using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class TreeXpLog
{
    public int LogId { get; set; }
    public int UserTreeId { get; set; }
    public int TaskId { get; set; }
    public ActivityType ActivityType { get; set; }
    public int XpAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required UserTree UserTree { get; set; }
    public required Tasks Task { get; set; }
}