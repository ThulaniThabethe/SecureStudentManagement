using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecureStudentManagement.Services
{
    public class StudentCloudStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public StudentCloudStorageService(IConfiguration config)
        {
            var conn = config["BlobStorageService:ServiceConnection"];
            var container = config["BlobStorageService:BlobContainer"];
            var client = new BlobServiceClient(conn);
            _containerClient = client.GetBlobContainerClient(container);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> SaveProfileImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (Array.IndexOf(allowedTypes, file.ContentType) < 0)
                throw new Exception("Only JPEG and PNG allowed");

            if (file.Length > 2 * 1024 * 1024)
                throw new Exception("File too large");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = _containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return GenerateSasUri(blob);
        }

        private string GenerateSasUri(BlobClient blob)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blob.BlobContainerName,
                BlobName = blob.Name,
                Resource = "b",
                ExpiresOn = DateTime.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sas = blob.GenerateSasUri(sasBuilder);
            return sas.ToString();
        }
    }
}