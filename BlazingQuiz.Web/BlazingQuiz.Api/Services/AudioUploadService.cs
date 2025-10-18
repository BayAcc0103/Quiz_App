using Microsoft.AspNetCore.StaticFiles;
using System.Security.Cryptography;

namespace BlazingQuiz.Api.Services
{
    public interface IAudioUploadService
    {
        Task<string> UploadAudioAsync(IFormFile file, string folderName = "quiz-audios");
        bool DeleteAudio(string audioPath);
    }

    public class AudioUploadService : IAudioUploadService
    {
        private readonly IWebHostEnvironment _environment;

        public AudioUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAudioAsync(IFormFile file, string folderName = "quiz-audios")
        {
            // Validate file
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            // Validate file type
            var allowedExtensions = new[] { ".mp3", ".wav", ".aac", ".m4a", ".ogg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only audio files (MP3, WAV, AAC, M4A, OGG) are allowed.");

            // Ensure folder exists
            var audiosPath = Path.Combine(_environment.WebRootPath, "audios", folderName);
            if (!Directory.Exists(audiosPath))
            {
                Directory.CreateDirectory(audiosPath);
            }

            // Generate unique filename
            var fileName = $"{GenerateUniqueFileName()}{extension}";
            var filePath = Path.Combine(audiosPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path from wwwroot
            return $"audios/{folderName}/{fileName}";
        }

        public bool DeleteAudio(string audioPath)
        {
            try
            {
                if (string.IsNullOrEmpty(audioPath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath, audioPath);
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