namespace ZenGarden.Domain.DTOs;

public class FinalizeTaskDto
{
    public int UserId { get; set; }
    public required string TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public int BaseXp { get; set; }
    public int TaskTypeId { get; set; }
    public int FocusMethodId { get; set; }
    public int Duration { get; set; }
    public int BreakTime { get; set; }
    public bool IsSuggested { get; set; }
}