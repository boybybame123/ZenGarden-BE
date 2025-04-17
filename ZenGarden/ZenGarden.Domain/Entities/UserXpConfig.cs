namespace ZenGarden.Domain.Entities;

public class UserXpConfig
{
    public int LevelId { get; set; }
    public int XpThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<UserExperience> UserExperiences { get; set; } = new List<UserExperience>();
}