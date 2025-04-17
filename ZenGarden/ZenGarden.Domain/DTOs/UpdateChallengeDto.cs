namespace ZenGarden.Domain.DTOs;

public class UpdateChallengeDto
{
    public int? ChallengeTypeId { get; set; }
    public string? ChallengeName { get; set; }
    public string? Description { get; set; }
    public int? Reward { get; set; }
    public int? MaxParticipants { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}