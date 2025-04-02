using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class UserXpLogDto
{
    public int LogId { get; init; }

    public int UserId { get; init; }
    public XpSourceType XpSource { get; init; }
    public int XpAmount { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public UserDto? User { get; init; }
}