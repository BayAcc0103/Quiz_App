using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuestionAudioEndpoints
    {
        public static IEndpointRouteBuilder MapQuestionAudioEndpoints(this IEndpointRouteBuilder app)
        {
            var questionAudioGroup = app.MapGroup("/api/question-audios").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload question audio
            questionAudioGroup.MapPost("/upload/{questionId:int}", async (
                int questionId, 
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
                    var audioPath = await audioUploadService.UploadAudioAsync(audio, "question-audios");
                    
                    // Find the question and update its audio path
                    var question = await context.Questions.FindAsync(questionId);
                    if (question == null)
                    {
                        return Results.NotFound($"Question with ID {questionId} not found");
                    }

                    // Log the current state
                    var originalAudioPath = question.AudioPath;
                    
                    // Delete old audio if it exists
                    if (!string.IsNullOrEmpty(question.AudioPath))
                    {
                        audioUploadService.DeleteAudio(question.AudioPath);
                    }

                    // Set the audio path
                    question.AudioPath = audioPath;
                    
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

            // Endpoint to remove question audio
            questionAudioGroup.MapDelete("/remove/{questionId:int}", async (
                int questionId, 
                IAudioUploadService audioUploadService, 
                QuizContext context) =>
            {
                var question = await context.Questions.FindAsync(questionId);
                if (question == null)
                    return Results.NotFound("Question not found");

                if (!string.IsNullOrEmpty(question.AudioPath))
                {
                    audioUploadService.DeleteAudio(question.AudioPath);
                    question.AudioPath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Audio removed successfully" });
            });

            return app;
        }
    }
}