namespace ZenGarden.Domain.Entities;

public class ChallengeType
{
    public int ChallengeTypeId { get; set; }
    public string? ChallengeTypeName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
}