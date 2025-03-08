namespace ZenGarden.Domain.Entities;

public class TaskFocusConfig
{
    public int TaskFocusSettingId { get; init; }
    public int TaskId { get; init; }
    public int FocusMethodId { get; init; }
    public int Duration { get; init; }
    public int BreakTime { get; init; }
    public bool IsSuggested { get; init; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Tasks? Tasks { get; init; }
    public FocusMethod? FocusMethod { get; init; }
}