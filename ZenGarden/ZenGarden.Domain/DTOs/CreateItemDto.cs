using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class CreateItemDto
    {
        [SwaggerSchema("ID của item (chỉ dùng khi cập nhật)", Nullable = true)]
        public int? ItemId { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [SwaggerSchema("Tên của item")]
        public string? Name { get; set; }

        [Required]
        [SwaggerSchema("Loại item (Enum)")]
        public ItemType Type { get; set; }

        [SwaggerSchema("Độ hiếm của item")]
        public string? Rarity { get; set; }

        [SwaggerSchema("Giá của item", Nullable = true)]
        public decimal? Cost { get; set; }

        [SwaggerSchema("File ảnh tải lên", Format = "binary")]
        public IFormFile? File { get; set; }

        [SwaggerSchema("Trạng thái của item")]
        public ItemStatus Status { get; set; }

        [SwaggerSchema("Chi tiết item", Nullable = true)]
        public virtual ItemDetailDto? ItemDetail { get; set; }



    }
}
