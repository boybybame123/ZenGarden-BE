using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs.FocusTracking;

public class CreateFocusActivityDto
{
    public int TrackingId { get; set; }
    public FocusActivityType Type { get; set; }
    public string? ActivityDetails { get; set; }
    public bool IsDistraction { get; set; }
    public int? Duration { get; set; }
}