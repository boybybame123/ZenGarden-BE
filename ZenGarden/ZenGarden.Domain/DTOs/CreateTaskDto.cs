using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class CreateTaskDto
{
    public int UserId { get; set; } 
    public int? WorkspaceId { get; set; }
    public int? UserTreeId { get; set; }

    public string TaskName { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public int Duration { get; set; } = 25; 

    public int FocusMethodId { get; set; } 
    
    public TaskType Type { get; set; }
    public int BaseXp { get; set; } = 50; 

    public int SuggestedBreak { get; set; } = 5;
    public int? CustomBreak { get; set; }
}