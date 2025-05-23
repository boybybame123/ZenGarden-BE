namespace ZenGarden.Domain.Entities;

public class UserConfig
{
    public int UserConfigId { get; set; }
    public int UserId { get; set; }
    public string? BackgroundConfig { get; set; }
    public string? SoundConfig { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual Users? User { get; set; }
}