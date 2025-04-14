using Microsoft.AspNetCore.Http;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IS3Service
{
    // Upload operations
    Task<string> UploadFileAsync(IFormFile file);
    Task<string> UploadFileToFolderAsync(IFormFile file, string folderName);

    // List operations
    Task<List<string>> ListFilesAsync();
    Task<List<string>> ListFilesInFolderAsync(string folderName);

    // File operations
    Task<Stream> DownloadFileAsync(string key);
    Task<bool> DeleteFileAsync(string key);

    // URL operations
    Task<string> GetPreSignedUrlAsync(string key, int expiryInMinutes = 60);
    string GetPublicUrl(string key);
}