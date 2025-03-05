﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public partial class UserTree
{
    public int UserTreeId { get; set; }

    public int? UserId { get; set; }

    public int? FinalTreeId { get; set; }

    public DateTime? PlantedAt { get; set; }

    public int TreeLevel { get; set; } = 1;
    
    public int TotalXp { get; set; } = 0;

    public TreeStatus TreeStatus { get; set; } = TreeStatus.Growing;

    public TreeRarity? FinalTreeRarity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual TreeType FinalTree { get; set; }
    
    public int LevelId { get; set; }

    public virtual ICollection<TradeHistory> TradeHistoryUserTreeA { get; set; } = new List<TradeHistory>();

    public virtual ICollection<TradeHistory> TradeHistoryUserTreeB { get; set; } = new List<TradeHistory>();
    
    public virtual Users User { get; set; }
}