﻿using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class ItemDto
{
    public int ItemId { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string Rarity { get; set; }

    public decimal? Cost { get; set; }



    public ItemStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ItemDetailDto? ItemDetail { get; set; }


}