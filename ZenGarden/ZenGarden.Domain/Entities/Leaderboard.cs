﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class Leaderboard
{
    public int LeaderboardId { get; set; }

    public int? UserId { get; set; }

    public int? TotalTrees { get; set; }

    public int? BestTrees { get; set; }

    public int? ProductivityScore { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Users User { get; set; }
}