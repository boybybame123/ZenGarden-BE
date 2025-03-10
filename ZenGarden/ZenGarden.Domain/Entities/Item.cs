﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public partial class Item
{
    public int ItemId { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string Rarity { get; set; }

    public decimal? Cost { get; set; }

    public bool? Limited { get; set; }

    public ItemStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<BagItem> BagItem { get; set; } = new List<BagItem>();
    
    public virtual ItemDetail ItemDetail { get; set; }

    public virtual ICollection<PurchaseHistory> PurchaseHistory { get; set; } = new List<PurchaseHistory>();

}