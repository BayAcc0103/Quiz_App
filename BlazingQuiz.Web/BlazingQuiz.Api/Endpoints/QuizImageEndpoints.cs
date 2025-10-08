using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuizImageEndpoints
    {
        public static IEndpointRouteBuilder MapQuizImageEndpoints(this IEndpointRouteBuilder app)
        {
            var quizImageGroup = app.MapGroup("/api/quiz-images").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload quiz image
            quizImageGroup.MapPost("/upload/{quizId:guid}", async (
                Guid quizId, 
                HttpRequest request,
                IImageUploadService imageUploadService, 
                QuizContext context) =>
            {
                // Extract the image file from the form data
                IFormFile? image = null;
                if (request.HasFormContentType && request.Form.Files.Count > 0)
                {
                    image = request.Form.Files["image"];
                }
                
                if (image == null || image.Length == 0)
                    return Results.BadRequest("No image file provided");

                try
                {
                    // Upload the image
                    var imagePath = await imageUploadService.UploadImageAsync(image, "quiz-images");
                    
                    // Find the quiz and update its image path
                    var quiz = await context.Quizzes.FindAsync(quizId);
                    if (quiz == null)
                        return Results.NotFound("Quiz not found");

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(quiz.ImagePath))
                    {
                        imageUploadService.DeleteImage(quiz.ImagePath);
                    }

                    quiz.ImagePath = imagePath;
                    await context.SaveChangesAsync();

                    return Results.Ok(new { imagePath, message = "Image uploaded successfully" });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while uploading the image");
                }
            });

            // Endpoint to remove quiz image
            quizImageGroup.MapDelete("/remove/{quizId:guid}", async (
                Guid quizId, 
                IImageUploadService imageUploadService, 
                QuizContext context) =>
            {
                var quiz = await context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return Results.NotFound("Quiz not found");

                if (!string.IsNullOrEmpty(quiz.ImagePath))
                {
                    imageUploadService.DeleteImage(quiz.ImagePath);
                    quiz.ImagePath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Image removed successfully" });
            });

            return app;
        }
    }
}