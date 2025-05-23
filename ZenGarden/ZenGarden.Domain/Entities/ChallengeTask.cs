namespace ZenGarden.Domain.Entities;

public class ChallengeTask
{
    public int ChallengeTaskId { get; set; }
    public int ChallengeId { get; set; }
    public int TaskId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual Challenge? Challenge { get; set; }
    public Tasks? Tasks { get; set; }
}