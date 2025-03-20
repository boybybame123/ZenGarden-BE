using System.ComponentModel.DataAnnotations;

namespace ZenGarden.Domain.DTOs;

public class FileObject
{
    [Required] public string FileName { get; set; }

    [Required] public string FileBase64 { get; set; } // 🔹 Chứa ảnh dưới dạng Base64

    public string Path { get; set; }
}