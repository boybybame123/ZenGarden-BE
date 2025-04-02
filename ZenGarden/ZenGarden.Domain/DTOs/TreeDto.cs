namespace ZenGarden.Domain.DTOs;

public class TreeDto
{
    public int TreeId { get; set; }
    public string? Name { get; set; }
    public string? Rarity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}