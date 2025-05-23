using Microsoft.AspNetCore.Http;

namespace ZenGarden.Domain.DTOs;

public class UpdateTaskDto
{
    public string? TaskName { get; set; }
    public int? TaskTypeId { get; set; }
    public int? UserTreeId { get; set; }
    public string? TaskDescription { get; set; }
    public string? TaskNote { get; set; }
    public string? TaskResult { get; set; }
    public IFormFile? TaskFile { get; set; }
    public int? TotalDuration { get; set; }
    public int? FocusMethodId { get; set; }
    public int? WorkDuration { get; set; }
    public int? BreakTime { get; set; }
    public double? AccumulatedTime { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}