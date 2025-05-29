using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TaskXpInfoDto
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public double BaseXp { get; set; }
    public double? BonusXp { get; set; }
    public string? BonusItemName { get; set; }
    public double TotalXp { get; set; }
    public ActivityType ActivityType { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Priority-related XP information
    public int? Priority { get; set; }
    public double? PriorityMultiplier { get; set; }
    public double? OriginalBaseXp { get; set; }
    public string? PriorityEffect { get; set; }
} 