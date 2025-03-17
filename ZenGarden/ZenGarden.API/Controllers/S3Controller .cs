using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ZenGarden.Core.Interfaces.IServices;
using Amazon.S3;

using ZenGarden.Domain.DTOs;

[ApiController]
[Route("api/s3")]
public class S3Controller : ControllerBase
{
    private readonly IS3Service _s3Service;

    public S3Controller(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

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
        var files = await _s3Service.ListFilesAsync();
        return Ok(files);
    }

    // 3. Download File
    [HttpGet("download/{key}")]
    public async Task<IActionResult> DownloadFile(string key)
    {
        var fileStream = await _s3Service.DownloadFileAsync(key);
        return File(fileStream, "application/octet-stream", key);
    }

    // 4. Delete File
    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> DeleteFile(string key)
    {
        var result = await _s3Service.DeleteFileAsync(key);
        return result ? Ok("Deleted Successfully") : BadRequest("Failed to delete file");
    }

    [HttpGet("presigned-url/{key}")]
    public async Task<IActionResult> GetPreSignedUrl(string key)
    {
        var url = await _s3Service.GetPreSignedUrlAsync(key);
        return Ok(new { Url = url });
    }
    [HttpPost("upload-file")]
    [Consumes("multipart/form-data")] // ⚠️ Bắt buộc có dòng này
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var url = await _s3Service.UploadFileAsync(file);
        return Ok(new { Url = url });
    }















}

