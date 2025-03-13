namespace ZenGarden.Domain.Entities;

public class XpConfig
{
    public int XpConfigId { get; set; }


    public int FocusMethodId { get; set; }


    public int TaskTypeId { get; set; }

    public double BaseXp { get; set; }
    public double Multiplier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual required FocusMethod FocusMethod { get; set; }
    public virtual required TaskType TaskType { get; set; }
}