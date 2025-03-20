using Microsoft.AspNetCore.Http;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs;

public class CreateItemDto
{
    public IFormFile File { get; set; }


    public int ItemId { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string Rarity { get; set; }

    public decimal? Cost { get; set; }


    public ItemStatus Status { get; set; }


    public int ItemDetailId { get; set; }


    public string Description { get; set; } // Mô tả item


    public string MediaUrl { get; set; } // Đường dẫn file ảnh hoặc nhạc

    public string Effect { get; set; } // Chứa JSON hiệu ứng

    public int? Duration { get; set; }
}