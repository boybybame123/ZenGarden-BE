namespace ZenGarden.Domain.DTOs;

public class FileObject
{
    public required string  FileName { get; set; }

    public required string FileBase64 { get; set; }

    public string? Path { get; set; }
}