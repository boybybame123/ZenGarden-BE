using System.ComponentModel.DataAnnotations;

namespace ZenGarden.Domain.DTOs;

/// <summary>
/// Data Transfer Object for Item Detail information
/// </summary>
public class ItemDetailDto
{
    /// <summary>
    /// Unique identifier for the item detail
    /// </summary>
    public int ItemDetailId { get; set; }

    /// <summary>
    /// Foreign key reference to the parent Item
    /// </summary>
    [Required(ErrorMessage = "Item ID is required")]
    public int ItemId { get; set; }

    /// <summary>
    /// Detailed description of the item
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// URL to the item's media file (image, audio, etc.)
    /// </summary>
    [Url(ErrorMessage = "Invalid media URL format")]
    public string? MediaUrl { get; set; }

    /// <summary>
    /// Effect configuration in JSON format
    /// </summary>
    [StringLength(1000, ErrorMessage = "Effect configuration cannot exceed 1000 characters")]
    public string? Effect { get; set; }

    /// <summary>
    /// Duration of the effect in seconds (null if permanent)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
    public int? Duration { get; set; }

    /// <summary>
    /// Number of times this item has been sold
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Sold count cannot be negative")]
    public int Sold { get; set; }

    /// <summary>
    /// Indicates if the item can only be purchased once
    /// </summary>
    public bool IsUnique { get; set; } = false;

    /// <summary>
    /// Maximum number of times this item can be purchased per month (null if unlimited)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Monthly purchase limit must be a positive number")]
    public int? MonthlyPurchaseLimit { get; set; }
}