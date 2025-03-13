namespace ZenGarden.Domain.DTOs.Response;
public class TreeResponse
{
    public int TreeId { get; set; }
    public string? Name { get; set; }
    public string? Rarity { get; set; }
    public int CurrentLevel { get; set; }
    public int Xp { get; set; }
    public int XpToNextLevel { get; set; }
}