using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TaskResponse(Tasks task)
{
    public int TaskId { get; set; } = task.TaskId;
    public string TaskName { get; set; } = task.TaskName;
    public string TaskDescription { get; set; } = task.TaskDescription;
    public int? WorkDuration { get; set; } = task.WorkDuration;
    public TasksStatus Status { get; set; } = task.Status;
    public DateTime? CreatedAt { get; set; } = task.CreatedAt;
    public DateTime? CompletedAt { get; set; } = task.CompletedAt;
}