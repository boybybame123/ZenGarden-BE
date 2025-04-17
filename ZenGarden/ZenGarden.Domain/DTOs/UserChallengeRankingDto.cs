namespace ZenGarden.Domain.DTOs;

public class UserChallengeRankingDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int CompletedTasks { get; set; }
    public bool IsWinner { get; set; }
}