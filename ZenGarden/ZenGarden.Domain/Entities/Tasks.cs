﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public partial class Tasks
{
    public int TaskId { get; set; }
    public int? UserId { get; set; }
    public int TaskTypeId { get; set; }  
    public int? UserTreeId { get; set; }
    public int TaskFocusSettingId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public int? Duration { get; set; }
    public int BaseXp { get; set; } = 50;
    public TaskType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TasksStatus Status { get; set; }
    public virtual Users User { get; set; }
    public virtual TaskFocusConfig TaskFocusConfig { get; set; }
    public virtual TaskType TaskType { get; set; }
    public virtual UserTree UserTree { get; set; }
}