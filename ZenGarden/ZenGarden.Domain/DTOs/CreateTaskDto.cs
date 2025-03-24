namespace ZenGarden.Domain.DTOs;

public class CreateTaskDto
{
    public int? FocusMethodId { get; set; }
    public int TaskTypeId { get; set; }
    public int? UserTreeId { get; set; }
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public int? TotalDuration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? WorkDuration { get; set; }
    public int? BreakTime { get; set; }
}