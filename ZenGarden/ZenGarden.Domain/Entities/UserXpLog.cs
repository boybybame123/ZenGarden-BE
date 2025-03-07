using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public sealed class UserXpLog
{
    public int LogId { get; init; }

    public int UserId { get; init; }
    public ActivityType ActivityType { get; init; }
    public int XpAmount { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Users? User { get; init; }
}