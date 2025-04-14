using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/s3")]
public class S3Controller(IS3Service s3Service) : ControllerBase
{
    // 1. Upload File
    //[HttpPost("upload")]
    //public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest("File is empty!");

    //    using var stream = file.OpenReadStream();
    //    var fileUrl = await _s3Service.UploadFileAsync(file.FileName, stream);
    //    return Ok(new { Url = fileUrl });
    //}

    // 2. List Files
    [HttpGet("list")]
    public async Task<IActionResult> ListFiles()
    {
        var files = await s3Service.ListFilesAsync();
        return Ok(files);
    }

    // 3. Download File
    [HttpGet("download/{key}")]
    public async Task<IActionResult> DownloadFile(string key)
    {
        var fileStream = await s3Service.DownloadFileAsync(key);
        return File(fileStream, "application/octet-stream", key);
    }

    // 4. Delete File
    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> DeleteFile(string key)
    {
        var result = await s3Service.DeleteFileAsync(key);
        return result ? Ok("Deleted Successfully") : BadRequest("Failed to delete file");
    }

    [HttpGet("resigned-url/{key}")]
    public async Task<IActionResult> GetPreSignedUrl(string key)
    {
        var url = await s3Service.GetPreSignedUrlAsync(key);
        return Ok(new { Url = url });
    }

    [HttpPost("upload-file")]
    [Consumes("multipart/form-data")] // ⚠️ Bắt buộc có dòng này
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var url = await s3Service.UploadFileAsync(file);
        return Ok(new { Url = url });
    }

    [HttpPost("upload-file-to-folder")]
    [Consumes("multipart/form-data")] // ⚠️ Bắt buộc có dòng này
    public async Task<IActionResult> UploadFileToFolder(IFormFile file, string folderName)
    {
        var url = await s3Service.UploadFileToFolderAsync(file, folderName);
        return Ok(new { Url = url });
    }

    [HttpGet("list-files-in-folder")]
    public async Task<IActionResult> ListFilesInFolder(string folderName)
    {
        var files = await s3Service.ListFilesInFolderAsync(folderName);
        return Ok(files);
    }

    [HttpGet("public-url/{key}")]
    public IActionResult GetPublicUrl(string key)
    {
        var url = s3Service.GetPublicUrl(key);
        return Ok(new { Url = url });
    }

    [HttpGet("download-file/{key}")]
    public async Task<IActionResult> DownloadFileFromS3(string key)
    {
        var stream = await s3Service.DownloadFileAsync(key);
        if (stream == null)
            return NotFound();
        return File(stream, "application/octet-stream", key);
    }
}