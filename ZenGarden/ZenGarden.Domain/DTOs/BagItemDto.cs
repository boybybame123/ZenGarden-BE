namespace ZenGarden.Domain.DTOs;

public class BagItemDto
{
    public int BagItemId { get; set; }

    public int? BagId { get; set; }

    public int? ItemId { get; set; }

    public int? Quantity { get; set; }

    public bool IsEquipped { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ItemDto? Item { get; set; }
}