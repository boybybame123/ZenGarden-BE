namespace ZenGarden.Domain.Entities;

public class XPConfig
{
    public int XPConfigId { get; set; }


    public int FocusMethodId { get; set; }


    public int TaskTypeId { get; set; }

    public double BaseXP { get; set; }
    public double Multiplier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual required FocusMethod FocusMethod { get; set; }
    public virtual required TaskType TaskType { get; set; }
}