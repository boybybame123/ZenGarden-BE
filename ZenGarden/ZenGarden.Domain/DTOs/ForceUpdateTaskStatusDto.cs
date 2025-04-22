using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class ForceUpdateTaskStatusDto
{
    public int TaskId { get; set; }
    public TasksStatus NewStatus { get; set; }
}