﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ZenGarden.Domain.Entities;

public partial class Tasks
{
    public int TaskId { get; set; }

    public int? UserId { get; set; }

    public string TaskName { get; set; }

    public string TaskDescription { get; set; }

    public int? Duration { get; set; }

    public string AiprocessedDescription { get; set; }

    public int? TimeOverdue { get; set; }

    public int? StatusId { get; set; }

    public int? Xpreward { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<Dailyrewardclaim> Dailyrewardclaim { get; set; } = new List<Dailyrewardclaim>();

    public virtual Taskstatus Status { get; set; }

    public virtual Users User { get; set; }

    public virtual ICollection<Useractivity> Useractivity { get; set; } = new List<Useractivity>();
}