using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services
{


    public class S3Service : IS3Service
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration config)
        {
            var awsOptions = config.GetSection("AWS");
            var accessKey = awsOptions["AccessKey"];
            var secretKey = awsOptions["SecretKey"];
            var serviceUrl = awsOptions["ServiceURL"];

            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
            _bucketName = awsOptions["BucketName"];
        }

        // 1. Upload File
        public async Task<string> UploadFileAsync(string key, Stream fileStream)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = "application/octet-stream",
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);
            return $"https://{_bucketName}.hn.ss.bfcplatform.vn/{key}";
        }

        // 2. Get File List
        public async Task<List<string>> ListFilesAsync()
        {
            var request = new ListObjectsV2Request { BucketName = _bucketName };
            var response = await _s3Client.ListObjectsV2Async(request);

            var files = new List<string>();
            foreach (var obj in response.S3Objects)
            {
                files.Add(obj.Key);
            }
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
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
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
    }

}
