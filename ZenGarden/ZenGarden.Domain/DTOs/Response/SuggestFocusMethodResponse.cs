namespace ZenGarden.Domain.DTOs.Response;

public class SuggestFocusMethodResponse
{
    public int FocusMethodId { get; set; }
    public string? FocusMethodName { get; set; }
    public int SuggestedDuration { get; set; }
    public int SuggestedBreak { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int? MinBreak { get; set; }
    public int? MaxBreak { get; set; }
}