using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class CreateItemDto
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; } // Loại item (background, music, xp_boost, xp_protect)

        public string Rarity { get; set; }

        public decimal? Cost { get; set; }

        public int ItemDetailId { get; set; }

        public string Description { get; set; } // Mô tả item

        public string MediaUrl { get; set; } // Đường dẫn file ảnh hoặc nhạc

        public string Effect { get; set; } // Chứa JSON hiệu ứng

        public int? Duration { get; set; } // Thời gian hiệu lực (giây), NULL nếu vĩnh viễn

      


    }
}
