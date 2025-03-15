namespace ZenGarden.Domain.DTOs;

public class UserTreeDto
{
    public int UserTreeId { get; set; } 
    public int UserId { get; set; }
    public string? UserName { get; set; }

    public string? Name { get; set; }
    public int LevelId { get; set; }
    public double XpToNextLevel { get; set; }
    public double TotalXp { get; set; } 
    public bool IsMaxLevel { get; set; } 
    public string? TreeStatus { get; set; } 
    
    public int? FinalTreeId { get; set; } 
    public string? FinalTreeName { get; set; }
    public string? FinalTreeRarity { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } 
}