using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs.FocusTracking;

public class UpdateFocusTrackingDto
{
    public DateTime? EndTime { get; set; }
    public int? ActualDuration { get; set; }
    public bool IsCompleted { get; set; }
    public string? TrackingNote { get; set; }
    public double XpEarned { get; set; }
    public FocusTrackingStatus Status { get; set; }
}