﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class Treeprogress
{
    public int ProgressId { get; set; }

    public int? UserTreeId { get; set; }

    public int? Xprequired { get; set; }

    public int? MaxTreesPerPeriod { get; set; }

    public string PeriodType { get; set; }

    public virtual Usertree UserTree { get; set; }
}