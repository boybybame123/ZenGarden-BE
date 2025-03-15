namespace ZenGarden.Domain.DTOs;

public class SuggestFocusMethodDto
{
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}