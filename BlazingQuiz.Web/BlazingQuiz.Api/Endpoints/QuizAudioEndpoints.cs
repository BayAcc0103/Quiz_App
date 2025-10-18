using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuizAudioEndpoints
    {
        public static IEndpointRouteBuilder MapQuizAudioEndpoints(this IEndpointRouteBuilder app)
        {
            var quizAudioGroup = app.MapGroup("/api/quiz-audios").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload quiz audio
            quizAudioGroup.MapPost("/upload/{quizId:guid}", async (
                Guid quizId, 
                HttpRequest request,
                IAudioUploadService audioUploadService, 
                QuizContext context) =>
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
                var allowedAudioTypes = new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/aac" };
                if (!allowedAudioTypes.Contains(audio.ContentType.ToLower()))
                {
                    return Results.BadRequest("Invalid audio file type. Only MP3 and other common audio formats are allowed.");
                }

                try
                {
                    // Upload the audio
                    var audioPath = await audioUploadService.UploadAudioAsync(audio, "quiz-audios");
                    
                    // Find the quiz and update its audio path
                    var quiz = await context.Quizzes.FindAsync(quizId);
                    if (quiz == null)
                    {
                        return Results.NotFound($"Quiz with ID {quizId} not found");
                    }

                    // Log the current state
                    var originalAudioPath = quiz.AudioPath;
                    
                    // Delete old audio if it exists
                    if (!string.IsNullOrEmpty(quiz.AudioPath))
                    {
                        audioUploadService.DeleteAudio(quiz.AudioPath);
                    }

                    // Set the audio path
                    quiz.AudioPath = audioPath;
                    
                    var rowsAffected = await context.SaveChangesAsync();

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

            // Endpoint to remove quiz audio
            quizAudioGroup.MapDelete("/remove/{quizId:guid}", async (
                Guid quizId, 
                IAudioUploadService audioUploadService, 
                QuizContext context) =>
            {
                var quiz = await context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return Results.NotFound("Quiz not found");

                if (!string.IsNullOrEmpty(quiz.AudioPath))
                {
                    audioUploadService.DeleteAudio(quiz.AudioPath);
                    quiz.AudioPath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Audio removed successfully" });
            });

            return app;
        }
    }
}