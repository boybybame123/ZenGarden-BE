namespace ZenGarden.Domain.DTOs;

public class TreeXpConfigDto
{
    public int LevelId { get; set; }
    public int XpThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<UserTreeDto> UserTrees { get; set; } = new List<UserTreeDto>();
}