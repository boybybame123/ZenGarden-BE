using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs.FocusTracking;
public class FocusActivityDto
{
    public int ActivityId { get; set; }
    public int TrackingId { get; set; }
    public FocusActivityType Type { get; set; }
    public string? ActivityDetails { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsDistraction { get; set; }
    public int? Duration { get; set; }
}