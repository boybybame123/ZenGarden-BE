using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class UpdateItemDetailDto
    {


        public IFormFile? File { get; set; }

        public int ItemId { get; set; } // Liên kết với bảng Item


        public string Description { get; set; } // Mô tả item




        public string Type { get; set; } 
        // Loại item (background, music, xp_boost, xp_protect)



        public string Effect { get; set; } // Chứa JSON hiệu ứng

        public int? Duration { get; set; }
    }
}
