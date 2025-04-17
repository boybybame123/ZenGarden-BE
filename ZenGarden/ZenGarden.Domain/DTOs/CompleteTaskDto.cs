using Microsoft.AspNetCore.Http;

namespace ZenGarden.Domain.DTOs;

public class CompleteTaskDto
{
    public int? UserTreeId { get; set; }
    public string? TaskNote { get; set; }
    public string? TaskResult { get; set; }
    public IFormFile? TaskFile { get; set; }
}