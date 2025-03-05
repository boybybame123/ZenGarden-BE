namespace ZenGarden.Domain.Entities;

public class TaskFocusSetting
{
    public int TaskFocusSettingId { get; set; }
    public int TaskId { get; set; }
    public int FocusMethodId { get; set; }

    public int SuggestedDuration { get; set; }
    public int? CustomDuration { get; set; }

    public int SuggestedBreak { get; set; }
    public int? CustomBreak { get; set; }

    public virtual required Tasks Task { get; set; }
    public virtual required FocusMethod FocusMethod { get; set; }
}