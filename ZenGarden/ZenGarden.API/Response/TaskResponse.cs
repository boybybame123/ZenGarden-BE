using ZenGarden.Domain.Entities;

namespace ZenGarden.API.Response;

public class TaskResponse(Tasks task)
{
    public int TaskId { get; set; } = task.TaskId;
    public string TaskName { get; set; } = task.TaskName;
    public string TaskDescription { get; set; } = task.TaskDescription;
    public int? Duration { get; set; } = task.Duration;
    public string AiProcessedDescription { get; set; } = task.AiProcessedDescription;
    public int? TimeOverdue { get; set; } = task.TimeOverdue;
    public int? Status { get; set; } = task.StatusId;
    public DateTime? CreatedAt { get; set; } = task.CreatedAt;
    public DateTime? CompletedAt { get; set; } = task.CompletedAt;
}