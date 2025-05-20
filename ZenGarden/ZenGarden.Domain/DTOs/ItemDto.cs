using System.ComponentModel.DataAnnotations;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

/// <summary>
/// Data Transfer Object for Item information
/// </summary>
public class ItemDto
{
    /// <summary>
    /// Unique identifier for the item
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// Name of the item
    /// </summary>
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Type of the item (e.g., XpBoostTree, Decoration, etc.)
    /// </summary>
    [Required(ErrorMessage = "Item type is required")]
    public ItemType Type { get; set; }

    /// <summary>
    /// Rarity level of the item (e.g., Common, Rare, Epic, Legendary)
    /// </summary>
    [Required(ErrorMessage = "Item rarity is required")]
    [StringLength(50, ErrorMessage = "Rarity cannot exceed 50 characters")]
    public string? Rarity { get; set; }

    /// <summary>
    /// Cost of the item in the marketplace
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
    public decimal? Cost { get; set; }

    /// <summary>
    /// Current status of the item (Active, Inactive)
    /// </summary>
    [Required(ErrorMessage = "Item status is required")]
    public ItemStatus Status { get; set; }

    /// <summary>
    /// Date and time when the item was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Detailed information about the item
    /// </summary>
    public virtual ItemDetailDto? ItemDetail { get; set; }
}