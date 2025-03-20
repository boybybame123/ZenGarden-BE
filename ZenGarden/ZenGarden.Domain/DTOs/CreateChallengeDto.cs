namespace ZenGarden.Domain.DTOs;

public class CreateChallengeDto
{
    public int ChallengeTypeId { get; set; }
    public string ChallengeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public List<CreateTaskDto>? Tasks { get; set; } 
}
