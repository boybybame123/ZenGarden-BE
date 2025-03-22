using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class ChallengeDto
{
    public int ChallengeId { get; set; }

    public int ChallengeTypeId { get; set; }
    public string? ChallengeName { get; set; }
    public string? Description { get; set; }
    public int Reward { get; set; }
    public ChallengeStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}