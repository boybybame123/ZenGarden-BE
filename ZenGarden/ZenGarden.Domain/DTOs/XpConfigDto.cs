namespace ZenGarden.Domain.DTOs;

public class XpConfigDto
{
    public int XpConfigId { get; set; }


    public int FocusMethodId { get; set; }


    public int TaskTypeId { get; set; }

    public double BaseXp { get; set; }
    public double XpMultiplier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}