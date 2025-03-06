using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public sealed class TreeXpLog
{
    [Key] public int LogId { get; set; }

    [Required] public int UserTreeId { get; set; }

    [Required] public int TaskId { get; set; }

    [Required] public ActivityType ActivityType { get; set; }

    [Required] public int XpAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserTreeId")] public required UserTree UserTree { get; set; }

    [ForeignKey("TaskId")] public required Tasks Task { get; set; }
}