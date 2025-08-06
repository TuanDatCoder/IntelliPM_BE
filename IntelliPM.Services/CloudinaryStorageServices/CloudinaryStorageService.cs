using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IntelliPM.Services.CloudinaryStorageServices
{
    public class CloudinaryStorageService : ICloudinaryStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Missing Cloudinary configuration (CloudName, ApiKey, or ApiSecret).");
            }

            var cloudinaryAccount = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(cloudinaryAccount);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string originalFileName)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException("File stream cannot be null or empty.");
            }

            string uniqueFileName = GenerateUniqueFileName(originalFileName);

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(originalFileName, fileStream),
                PublicId = uniqueFileName,
                Overwrite = true,
                Type = "upload"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Failed to upload file to Cloudinary: {uploadResult.Error?.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            string uniqueId = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(originalFileName);
            return $"products/{uniqueId}{extension}";
        }
    }
}