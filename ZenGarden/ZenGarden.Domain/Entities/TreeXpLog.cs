using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class TreeXpLog
{
    public int LogId { get; set; }
    public int? TaskId { get; set; }
    public ActivityType ActivityType { get; set; }
    public double XpAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Tasks? Tasks { get; set; }
}