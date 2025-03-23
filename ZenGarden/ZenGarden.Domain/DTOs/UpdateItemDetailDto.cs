using Microsoft.AspNetCore.Http;

namespace ZenGarden.Domain.DTOs;

public class UpdateItemDetailDto
{
    public IFormFile? File { get; set; }

    public int ItemDetailId { get; set; }

    public int ItemId { get; set; } // Liên kết với bảng Item


    public string Description { get; set; } // Mô tả item


    public string Type { get; set; } // Loại item (background, music, xp_boost, xp_protect)

    public string MediaUrl { get; set; } // Đường dẫn file ảnh hoặc nhạc

    public string Effect { get; set; } 

    public int? Duration { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn



    public bool IsUnique { get; set; } = false; // TRUE nếu chỉ mua 1 lần

    public int? MonthlyPurchaseLimit { get; set; } = 0; // Chứa JSON hiệu ứng
}