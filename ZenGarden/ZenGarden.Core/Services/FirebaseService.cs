using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public FirebaseStorageService(StorageClient storageClient, IConfiguration configuration)
        {
            _storageClient = storageClient;
            _configuration = configuration;
            _bucketName = _configuration["Firebase:Bucket"]!;
        }

        public async Task DeleteImageAsync(string imageName)
        {
            await _storageClient.DeleteObjectAsync(_bucketName, imageName, cancellationToken: CancellationToken.None);
        }

        public string GetImageUrl(string imageName)
        {

            string imageUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(imageName)}?alt=media";
            return imageUrl;
        }

        public async Task<string> UpdateImageAsync(IFormFile imageFile, string imageName)
        {

            using var stream = new MemoryStream();

            await imageFile.CopyToAsync(stream);

            stream.Position = 0;

            // Re-upload the image with the same name to update it
            var blob = await _storageClient.UploadObjectAsync(_bucketName, imageName, imageFile.ContentType, stream, cancellationToken: CancellationToken.None);

            return GetImageUrl(imageName);
        }


        public async Task<string> UploadImageAsync(IFormFile imageFile, string? imageName = default)
        {
            imageName ??= $"{Path.GetFileNameWithoutExtension(imageFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            //            imageName += $"_{Guid.NewGuid()}";
            using var stream = new MemoryStream();

            await imageFile.CopyToAsync(stream);

            var blob = await _storageClient.UploadObjectAsync(_bucketName, imageName, imageFile.ContentType, stream, cancellationToken: CancellationToken.None);

            if (blob is null)
            {
                throw new Exception("Upload image failed");
            }

            return GetImageUrl(imageName);

        }

        public async Task<string[]> UploadImagesAsync(IFormFileCollection files)
        {
            var uploadTasks = new List<Task<string>>();

            foreach (var file in files)
            {
                uploadTasks.Add(UploadImageAsync(file));
            }

            var imageUrls = await Task.WhenAll(uploadTasks);

            return imageUrls;
        }
        public string ExtractImageNameFromUrl(string imageUrl)
        {
            // Find the position of 'o/' in the URL
            int start = imageUrl.IndexOf("o/") + 2;  // +2 to skip past 'o/'

            // Find the position of '?alt=' which marks the end of the image name
            int end = imageUrl.IndexOf("?alt=media");

            // Extract the image name from the URL
            string imageName = imageUrl.Substring(start, end - start);

            return imageName;
        }
    }
}
