namespace ZenGarden.Domain.DTOs;

public class UpdateChallengeDto
{
    public int ChallengeId { get; set; }
    public int ChallengeTypeId { get; set; }
    public string ChallengeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Reward { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}