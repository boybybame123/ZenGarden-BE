﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class Focusmethod
{
    public int FocusMethodId { get; set; }

    public string Name { get; set; }

    public int? DefaultDuration { get; set; }

    public int? DefaultBreak { get; set; }

    public int? MinDuration { get; set; }

    public int? MaxDuration { get; set; }

    public int? MinBreak { get; set; }

    public int? MaxBreak { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Useractivity> Useractivity { get; set; } = new List<Useractivity>();
}