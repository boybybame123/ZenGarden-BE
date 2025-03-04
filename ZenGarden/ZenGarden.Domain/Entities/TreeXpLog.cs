using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class TreeXpLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public int UserTreeId { get; set; }

    public int? TaskId { get; set; } 

    [Required]
    public ActivityType ActivityType { get; set; }

    [Required]
    public int XpAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExternalTask { get; set; } = false;

    [ForeignKey("UserTreeId")]
    public virtual required UserTree UserTree { get; set; }

    [ForeignKey("TaskId")]
    public virtual required Tasks Task { get; set; }
}