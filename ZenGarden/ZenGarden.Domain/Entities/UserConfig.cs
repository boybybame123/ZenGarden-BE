namespace ZenGarden.Domain.Entities;

public class UserConfig
{
    public int UserConfigId { get; set; }
    public int UserId { get; set; }
    public string? BackgroundConfig { get; set; }
    public string? SoundConfig { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual required Users User { get; set; }
}