using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazingQuiz.Api.Endpoints
{
    public static class CategoryImageEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryImageEndpoints(this IEndpointRouteBuilder app)
        {
            var categoryImageGroup = app.MapGroup("/api/category-images").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            // Endpoint to upload category image
            categoryImageGroup.MapPost("/upload/{categoryId:int}", async (
                int categoryId, 
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
                    var imagePath = await imageUploadService.UploadImageAsync(image, "category-images");
                    
                    // Find the category and update its image path
                    var category = await context.Categories.FindAsync(categoryId);
                    if (category == null)
                        return Results.NotFound("Category not found");

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(category.ImagePath))
                    {
                        imageUploadService.DeleteImage(category.ImagePath);
                    }

                    category.ImagePath = imagePath;
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

            // Endpoint to remove category image
            categoryImageGroup.MapDelete("/remove/{categoryId:int}", async (
                int categoryId, 
                IImageUploadService imageUploadService, 
                QuizContext context) =>
            {
                var category = await context.Categories.FindAsync(categoryId);
                if (category == null)
                    return Results.NotFound("Category not found");

                if (!string.IsNullOrEmpty(category.ImagePath))
                {
                    imageUploadService.DeleteImage(category.ImagePath);
                    category.ImagePath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Image removed successfully" });
            });

            return app;
        }
    }
}