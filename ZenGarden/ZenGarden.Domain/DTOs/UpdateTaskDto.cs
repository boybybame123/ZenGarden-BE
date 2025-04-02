using Microsoft.AspNetCore.Http;

namespace ZenGarden.Domain.DTOs;

public class UpdateTaskDto
{
    public int TaskId { get; set; }

    public string? TaskName { get; set; }

    public string? TaskDescription { get; set; }

    public string? TaskNote { get; set; }

    public string? TaskResult { get; set; }
    public IFormFile? TaskFile { get; set; }

    public int? TotalDuration { get; set; }
    public int? FocusMethodId { get; set; }

    public int? WorkDuration { get; set; }

    public int? BreakTime { get; set; }

    private IFormFile? TaskResultFile { get; set; } = null;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}