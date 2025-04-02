using Microsoft.AspNetCore.Http;

namespace ZenGarden.Domain.DTOs;

public class UpdateItemDetailDto
{
    public int ItemId { get; set; } // Liên kết với bảng Item


    public string? Description { get; set; } // Mô tả item


    public IFormFile? File { get; set; }

    public string? Effect { get; set; }

    public int? Duration { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn


    public bool IsUnique { get; set; } = false; // TRUE nếu chỉ mua 1 lần

    public int? MonthlyPurchaseLimit { get; set; } = 0; // Chứa JSON hiệu ứng
}