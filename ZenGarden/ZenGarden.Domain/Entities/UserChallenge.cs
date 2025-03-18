using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class UserChallenge
{
    public int UserChallengeId { get; set; }
    public int ChallengeId { get; set; }
    public int UserId { get; set; }
    public int Progress { get; set; }
    public UserChallengeRole ChallengeRole { get; set; } = 0;
    public UserChallengeStatus Status { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual Challenge? Challenge { get; set; }
    public virtual Users? User { get; set; }
}