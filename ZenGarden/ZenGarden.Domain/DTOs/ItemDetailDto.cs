namespace ZenGarden.Domain.DTOs;

public class ItemDetailDto
{
    public int ItemDetailId { get; set; }

    public int ItemId { get; set; } // Liên kết với bảng Item


    public string? Description { get; set; } // Mô tả item


    public string? MediaUrl { get; set; } // Đường dẫn file ảnh hoặc nhạc

    public string? Effect { get; set; } // Chứa JSON hiệu ứng

    public int? Duration { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn

    public int Sold { get; set; } // Số lần bán 

    public bool IsUnique { get; set; } = false; // TRUE nếu chỉ mua 1 lần

    public int? MonthlyPurchaseLimit { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn
}