namespace ZenGarden.Domain.DTOs;

public class FocusMethodDto
{
    public int FocusMethodId { get; set; }
    public string? FocusMethodName { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int? MinBreak { get; set; }
    public int? MaxBreak { get; set; }
    public int? DefaultDuration { get; set; }
    public int? DefaultBreak { get; set; }
}