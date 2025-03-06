namespace ZenGarden.Domain.DTOs;

public class CreateTaskDto
{
    public required string TaskName { get; set; }
    public string? TaskDescription { get; set; }
}