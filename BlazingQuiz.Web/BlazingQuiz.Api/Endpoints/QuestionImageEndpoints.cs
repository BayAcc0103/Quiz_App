using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuestionImageEndpoints
    {
        public static IEndpointRouteBuilder MapQuestionImageEndpoints(this IEndpointRouteBuilder app)
        {
            var questionImageGroup = app.MapGroup("/api/question-images").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload question image
            questionImageGroup.MapPost("/upload/{questionId:int}", async (
                int questionId, 
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
                    var imagePath = await imageUploadService.UploadImageAsync(image, "question-images");
                    
                    // Find the question and update its image path
                    var question = await context.Questions.FindAsync(questionId);
                    if (question == null)
                        return Results.NotFound("Question not found");

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(question.ImagePath))
                    {
                        imageUploadService.DeleteImage(question.ImagePath);
                    }

                    question.ImagePath = imagePath;
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

            // Endpoint to remove question image
            questionImageGroup.MapDelete("/remove/{questionId:int}", async (
                int questionId, 
                IImageUploadService imageUploadService, 
                QuizContext context) =>
            {
                var question = await context.Questions.FindAsync(questionId);
                if (question == null)
                    return Results.NotFound("Question not found");

                if (!string.IsNullOrEmpty(question.ImagePath))
                {
                    imageUploadService.DeleteImage(question.ImagePath);
                    question.ImagePath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Image removed successfully" });
            });

            return app;
        }
    }
}