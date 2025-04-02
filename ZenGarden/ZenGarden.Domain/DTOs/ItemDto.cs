using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class ItemDto
{
    public int ItemId { get; set; }

    public string? Name { get; set; }

    public ItemType Type { get; set; }

    public string? Rarity { get; set; }

    public decimal? Cost { get; set; }


    public ItemStatus Status { get; set; }


    public virtual ItemDetailDto? ItemDetail { get; set; }
}