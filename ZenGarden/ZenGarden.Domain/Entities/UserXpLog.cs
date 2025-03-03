using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.Entities;

public class UserXpLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public ActivityType ActivityType { get; set; } // TASK_XP, WEB_XP, DAILY_XP, STREAK_XP

    [Required]
    public int XpAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual Users? User { get; set; }
}
