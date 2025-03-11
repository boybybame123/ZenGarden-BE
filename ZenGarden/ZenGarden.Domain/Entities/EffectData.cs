namespace ZenGarden.Domain.Entities;

public class EffectData
{
    public double? XpMultiplier { get; set; } // Hệ số tăng XP (nếu có)
    public string? ProtectionDuration { get; set; } // Thời gian bảo vệ XP (nếu có)
}