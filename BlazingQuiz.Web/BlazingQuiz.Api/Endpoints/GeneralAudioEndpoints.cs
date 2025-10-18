using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazingQuiz.Api.Endpoints
{
    public static class GeneralAudioEndpoints
    {
        public static IEndpointRouteBuilder MapGeneralAudioEndpoints(this IEndpointRouteBuilder app)
        {
            var generalAudioGroup = app.MapGroup("/api/audio-upload").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload general audio
            generalAudioGroup.MapPost("/upload", async (
                HttpRequest request,
                IAudioUploadService audioUploadService,
                string folder = "general-audios") =>
            {
                // Extract the audio file from the form data
                IFormFile? audio = null;
                if (request.HasFormContentType && request.Form.Files.Count > 0)
                {
                    audio = request.Form.Files["audio"];
                }
                
                if (audio == null || audio.Length == 0)
                    return Results.BadRequest("No audio file provided");

                // Validate file type
                var allowedAudioTypes = new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/aac", "audio/ogg", "audio/m4a" };
                if (!allowedAudioTypes.Contains(audio.ContentType.ToLower()))
                {
                    return Results.BadRequest("Invalid audio file type. Only MP3 and other common audio formats are allowed.");
                }

                try
                {
                    // Upload the audio
                    var audioPath = await audioUploadService.UploadAudioAsync(audio, folder);
                    
                    return Results.Ok(new { audioPath, message = "Audio uploaded successfully" });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    // Log the full exception for debugging
                    return Results.Problem($"An error occurred while uploading the audio: {ex.Message}");
                }
            });

            return app;
        }
    }
}