using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class S3Service : IS3Service
{
    private readonly string _baseUrl;
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;
    private readonly string _serviceUrl;

    public S3Service(IConfiguration config)
    {
        var awsSection = config.GetSection("AWS");
        var accessKey = awsSection["AccessKey"] ?? throw new ArgumentNullException("AccessKey missing");
        var secretKey = awsSection["SecretKey"] ?? throw new ArgumentNullException("SecretKey missing");
        _serviceUrl = awsSection["ServiceURL"] ?? throw new ArgumentNullException("ServiceURL missing");
        _bucketName = awsSection["BucketName"] ?? throw new ArgumentNullException("BucketName missing");

        // Xác định base URL từ service URL
        var uri = new Uri(_serviceUrl);
        _baseUrl = "https://zengarden.hcm.ss.bfcplatform.vn";

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _serviceUrl,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        return await UploadFileInternalAsync(file, string.Empty);
    }

    public async Task<string> UploadFileToFolderAsync(IFormFile file, string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            throw new ArgumentException("Folder name cannot be empty");

        return await UploadFileInternalAsync(file, folderName);
    }

    public async Task<List<string>> ListFilesAsync()
    {
        var request = new ListObjectsV2Request { BucketName = _bucketName };
        var response = await _s3Client.ListObjectsV2Async(request);

        return response.S3Objects
            .Select(obj => $"{_baseUrl}/{obj.Key}")
            .ToList();
    }

    public async Task<List<string>> ListFilesInFolderAsync(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            throw new ArgumentException("Folder name cannot be empty");

        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = $"{folderName.Trim('/')}/"
        };

        var response = await _s3Client.ListObjectsV2Async(request);

        return response.S3Objects
            .Select(obj => $"{_baseUrl}/{obj.Key}")
            .ToList();
    }

    public async Task<string> UploadFileToTaskUserFolderAsync(IFormFile file, int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be a positive integer");

        var folderPath = $"Task/{userId}"; // Task cố định
        return await UploadFileInternalAsync(file, folderPath);
    }





    public async Task<Stream> DownloadFileAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty");

        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty");

        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }

    public async Task<string> GetPreSignedUrlAsync(string key, int expiryInMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty");

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public string GetPublicUrl(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty");

        return $"{_baseUrl}/{key}";
    }

    private async Task<string> UploadFileInternalAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        // Tạo tên file ngẫu nhiên để tránh trùng lặp
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var key = string.IsNullOrEmpty(folderName)
            ? fileName
            : $"{folderName.Trim('/')}/{fileName}";

        await using var stream = file.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.PublicRead,
            AutoCloseStream = true,
            PartSize = 10 * 1024 * 1024 // 10MB
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return $"{_baseUrl}/{key}";
    }






}