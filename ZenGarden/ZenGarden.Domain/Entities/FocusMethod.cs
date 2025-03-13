﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class FocusMethod
{
    public int FocusMethodId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? DefaultDuration { get; set; }
    public int? DefaultBreak { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int? MinBreak { get; set; }
    public int? MaxBreak { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<XpConfig> XPConfigs { get; set; } = new List<XpConfig>();
    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}