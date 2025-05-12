using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TaskDto
{
    public int TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public int? TotalDuration { get; set; }
    public int? WorkDuration { get; set; }
    public int? BreakTime { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TasksStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? TaskNote { get; set; }
    public string? TaskResult { get; set; }
    public string? RemainingTime { get; set; }
    public string? AccumulatedTime { get; set; }
    public int? Priority { get; set; }
    public string? UserTreeName { get; set; }
    public string? TaskTypeName { get; set; }
    public string? FocusMethodName { get; set; }

    
}