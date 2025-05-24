using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs.FocusTracking;

public class FocusTrackingDto
{
    public int TrackingId { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public int FocusMethodId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int PlannedDuration { get; set; }
    public int? ActualDuration { get; set; }
    public bool IsCompleted { get; set; }
    public string? TrackingNote { get; set; }
    public double XpEarned { get; set; }
    public FocusTrackingStatus Status { get; set; }
    public List<FocusActivityDto>? Activities { get; set; }
}