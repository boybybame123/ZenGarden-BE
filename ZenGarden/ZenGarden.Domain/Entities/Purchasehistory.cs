﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class Purchasehistory
{
    public int PurchaseId { get; set; }

    public int? UserId { get; set; }

    public int? ItemId { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; }

    public virtual Item Item { get; set; }

    public virtual Users User { get; set; }
}