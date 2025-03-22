using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class Challenge
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

    public ChallengeType? ChallengeType { get; set; }
    public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    public ICollection<ChallengeTask> ChallengeTasks { get; set; } = new List<ChallengeTask>();
}