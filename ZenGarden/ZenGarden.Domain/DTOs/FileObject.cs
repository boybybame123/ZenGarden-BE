using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class FileObject
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileBase64 { get; set; }  // 🔹 Chứa ảnh dưới dạng Base64

        public string Path { get; set; }
    }
}
