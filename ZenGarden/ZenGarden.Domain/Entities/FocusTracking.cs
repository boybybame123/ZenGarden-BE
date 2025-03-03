using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZenGarden.Domain.Entities;

public class FocusTracking
{
    [Key]
    public int FocusId { get; set; }

    [Required]
    public int UserId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    public int FocusMethodId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? UserAdjustedDuration { get; set; }

    [Required]
    public int SuggestedDuration { get; set; }

    public int Interruptions { get; set; } = 0;

    public int KeyboardUsage { get; set; } = 0;

    public int MouseUsage { get; set; } = 0;

    [Required]
    public int ActiveTime { get; set; }

    public int XpFocusReward { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual Users? User { get; set; }

    [ForeignKey("TaskId")]
    public virtual Tasks? Task { get; set; } 

    [ForeignKey("FocusMethodId")]
    public virtual FocusMethod? FocusMethod { get; set; }

}
