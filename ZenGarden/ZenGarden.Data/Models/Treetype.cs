﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Data.Models;

public partial class Treetype
{
    public int TreeTypeId { get; set; }

    public string Name { get; set; }

    public string Rarity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? BasePrice { get; set; }

    public virtual ICollection<Usertree> Usertrees { get; set; } = new List<Usertree>();
}