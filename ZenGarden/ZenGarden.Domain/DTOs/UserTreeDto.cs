using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class UserTreeDto
{
    public int UserId { get; set; }
    public int? FinalTreeId { get; set; }
    public int LevelId { get; set; } = 1;
    public int TotalXp { get; set; } = 0;
    public bool IsMaxLevel { get; set; } = false;
    public TreeStatus TreeStatus { get; set; } = TreeStatus.Growing;
}