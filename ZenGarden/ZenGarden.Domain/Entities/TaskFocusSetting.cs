namespace ZenGarden.Domain.Entities;

public sealed class TaskFocusSetting
{
    public int TaskFocusSettingId { get; set; }
    public int TaskId { get; set; }
    public int FocusMethodId { get; set; }

    public int SuggestedDuration { get; set; }
    public int? CustomDuration { get; set; }

    public int SuggestedBreak { get; set; }
    public int? CustomBreak { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public required Tasks Task { get; set; }
    public required FocusMethod FocusMethod { get; set; }
}