using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class TaskDto
{
    public int TaskId { get; set; }

    public int? UserId { get; set; }

    public string? TaskName { get; set; }

    public string? TaskDescription { get; set; }

    public int? WorkDuration { get; set; } 

    public int? BreakTime { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? StartDate { get; set; }  

    public DateTime? EndDate { get; set; }

    public TasksStatus Status { get; set; }

    public string? TaskNote { get; set; }  

    public string? TaskResult { get; set; } 
    
    public bool? IsSuggested { get; set; }

    public Users? User { get; set; }
}