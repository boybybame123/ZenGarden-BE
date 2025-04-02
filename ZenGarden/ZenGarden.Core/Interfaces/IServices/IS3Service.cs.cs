using Microsoft.AspNetCore.Http;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IS3Service
{
    /// <summary>
    ///     Uploads a file to the S3 bucket.
    /// </summary>
    /// <param name="key">The unique key (filename) in the bucket.</param>
    /// <param name="fileStream">The file stream.</param>
    /// <returns>The public URL of the uploaded file.</returns>
    Task<string> UploadFileAsync(IFormFile file);

    /// <summary>
    ///     Retrieves a list of all file keys in the bucket.
    /// </summary>
    /// <returns>List of file keys.</returns>
    Task<List<string>> ListFilesAsync();

    /// <summary>
    ///     Downloads a file from the S3 bucket.
    /// </summary>
    /// <param name="key">The unique key (filename) in the bucket.</param>
    /// <returns>The file stream.</returns>
    Task<Stream> DownloadFileAsync(string key);

    /// <summary>
    ///     Deletes a file from the S3 bucket.
    /// </summary>
    /// <param name="key">The unique key (filename) in the bucket.</param>
    /// <returns>True if deleted successfully, otherwise false.</returns>
    Task<bool> DeleteFileAsync(string key);


    Task<string> GetPreSignedUrlAsync(string key, int expiryInMinutes = 60);
}