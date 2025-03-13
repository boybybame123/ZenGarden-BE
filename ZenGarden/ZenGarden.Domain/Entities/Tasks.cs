﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public partial class Tasks
{
    
    public int TaskId { get; set; }
    public int TaskTypeId { get; set; }  
    public int? UserTreeId { get; set; }
    public int? FocusMethodId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public int? WorkDuration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? StartDate { get; set; }  
    public DateTime? EndDate { get; set; }
    public TasksStatus Status { get; set; }
    public int BreakTime { get; set; }
    public bool IsSuggested { get; set; } = true;
    public string TaskNote { get; set; } 
    public string TaskResult { get; set; } 
    public virtual FocusMethod FocusMethod { get; set; }
    public virtual TaskType TaskType { get; set; }
    public virtual UserTree UserTree { get; set; }
    public virtual ICollection<TreeXpLog> TreeXpLog { get; set; }
    public virtual ICollection<ChallengeTask> ChallengeTasks { get; set; } = new List<ChallengeTask>();
}