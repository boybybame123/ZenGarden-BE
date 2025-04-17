namespace ZenGarden.Domain.Entities;

public class Packages
{
    public int PackageId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Amount { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}