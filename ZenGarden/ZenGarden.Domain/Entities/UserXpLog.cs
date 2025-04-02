using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class UserXpLog
{
    public int LogId { get; init; }
    public int UserId { get; init; }
    public XpSourceType XpSource { get; init; }
    public double XpAmount { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Users? User { get; init; }
}