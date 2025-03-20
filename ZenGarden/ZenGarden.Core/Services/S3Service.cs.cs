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
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;

    public S3Service(IConfiguration config)
    {
        var awsSection = config.GetSection("AWS");
        var accessKey = awsSection["AccessKey"];
        var secretKey = awsSection["SecretKey"];
        var serviceUrl = awsSection["ServiceURL"];

        // Cấu hình S3 cho BizflyCloud
        var s3Config = new AmazonS3Config
        {
            ServiceURL = serviceUrl, // VD: "https://hcm.ss.bfcplatform.vn"
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        _bucketName = awsSection["BucketName"];
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var key = file.FileName;
        var uploadRequest = new TransferUtilityUploadRequest
        {
            BucketName = _bucketName,
            InputStream = stream,
            Key = key,
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.PublicRead,
            PartSize = 10 * 1024 * 1024, // Set chunk size for large files
            AutoCloseStream = true
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return GeneratePreSignedUrl(key);
    }


    // 2. Get File List
    public async Task<List<string>> ListFilesAsync()
    {
        var request = new ListObjectsV2Request { BucketName = _bucketName };
        var response = await _s3Client.ListObjectsV2Async(request);

        var files = new List<string>();
        foreach (var obj in response.S3Objects) files.Add(obj.Key);

        return files;
    }

    // 3. Download File
    public async Task<Stream> DownloadFileAsync(string key)
    {
        var request = new GetObjectRequest { BucketName = _bucketName, Key = key };
        var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    // 4. Delete File
    public async Task<bool> DeleteFileAsync(string key)
    {
        var request = new DeleteObjectRequest { BucketName = _bucketName, Key = key };
        var response = await _s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }

    public async Task<string> GetPreSignedUrlAsync(string key, int expiryInMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    private string GeneratePreSignedUrl(string fileKey, int expiryDuration = 3600)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.AddSeconds(expiryDuration) // Hết hạn sau 1 giờ
        };

        return _s3Client.GetPreSignedURL(request);
    }
}