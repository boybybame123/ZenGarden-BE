﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Data.Models;

public partial class Itemdetail
{
    public int ItemDetailId { get; set; }

    public int? ItemId { get; set; }

    public string Description { get; set; }

    public string ImageUrl { get; set; }

    public string Stats { get; set; }

    public string Requirements { get; set; }

    public string SpecialEffects { get; set; }

    public string DurationType { get; set; }

    public int? Duration { get; set; }

    public int? Cooldown { get; set; }

    public int? MaxStack { get; set; }

    public string Tags { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Item Item { get; set; }
}