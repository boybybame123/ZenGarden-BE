using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TreeXpLogDto
{
    public int LogId { get; set; }
    public int? TaskId { get; set; }
    public ActivityType ActivityType { get; set; }
    public int XpAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public TaskDto Tasks { get; set; }
}