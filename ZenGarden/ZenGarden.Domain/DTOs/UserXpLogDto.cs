using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class UserXpLogDto
{
    public int LogId { get; init; }

    public int UserId { get; init; }
    public XpSourceType XpSource { get; init; }
    public double XpAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public UserDto? User { get; init; }
}