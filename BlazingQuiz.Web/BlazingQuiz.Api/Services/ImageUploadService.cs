using Microsoft.AspNetCore.StaticFiles;
using System.Security.Cryptography;

namespace BlazingQuiz.Api.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName = "category-images");
        bool DeleteImage(string imagePath);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName = "category-images")
        {
            // Validate file
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            // Ensure folder exists
            var imagesPath = Path.Combine(_environment.WebRootPath, "images", folderName);
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            // Generate unique filename
            var fileName = $"{GenerateUniqueFileName()}{extension}";
            var filePath = Path.Combine(imagesPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path from wwwroot
            return $"images/{folderName}/{fileName}";
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateUniqueFileName()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "");
            }
        }
    }
}