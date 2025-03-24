namespace ZenGarden.Domain.DTOs;

public class UserChallengeProgressDto
{
    public int ChallengeId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int CompletedTasks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}