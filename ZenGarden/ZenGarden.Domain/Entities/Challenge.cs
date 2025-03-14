using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class Challenge
{
    public int ChallengeId { get; set; }

    public int ChallengeTypeId { get; set; }
    public string? ChallengeName { get; set; }
    public string? Description { get; set; }
    public int XpReward { get; set; }
    public TasksStatus status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual required ChallengeType ChallengeType { get; set; }
    public virtual ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    public virtual ICollection<ChallengeTask> ChallengeTasks { get; set; } = new List<ChallengeTask>();
}