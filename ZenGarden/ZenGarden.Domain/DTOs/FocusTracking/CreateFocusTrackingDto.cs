namespace ZenGarden.Domain.DTOs.FocusTracking;

public class CreateFocusTrackingDto
{
    public int TaskId { get; set; }
    public int FocusMethodId { get; set; }
    public int PlannedDuration { get; set; }
}