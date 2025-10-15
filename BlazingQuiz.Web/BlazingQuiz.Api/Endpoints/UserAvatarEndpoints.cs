using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class UserAvatarEndpoints
    {
        public static IEndpointRouteBuilder MapUserAvatarEndpoints(this IEndpointRouteBuilder app)
        {
            var userAvatarGroup = app.MapGroup("/api/user-avatars").RequireAuthorization();

            // Endpoint to upload user avatar
            userAvatarGroup.MapPost("/upload", async (
                HttpRequest request,
                IImageUploadService imageUploadService,
                QuizContext context,
                ClaimsPrincipal user) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Extract the image file from the form data
                IFormFile? image = null;
                if (request.HasFormContentType && request.Form.Files.Count > 0)
                {
                    image = request.Form.Files["avatar"];
                }
                
                if (image == null || image.Length == 0)
                    return Results.BadRequest("No avatar file provided");

                try
                {
                    // Upload the image to user-avatars folder
                    var imagePath = await imageUploadService.UploadImageAsync(image, "user-avatars");
                    
                    // Find the user and update their avatar path
                    var dbUser = await context.Users.FindAsync(userId);
                    if (dbUser == null)
                        return Results.NotFound("User not found");

                    // Delete old avatar if it exists
                    if (!string.IsNullOrEmpty(dbUser.AvatarPath))
                    {
                        imageUploadService.DeleteImage(dbUser.AvatarPath);
                    }

                    dbUser.AvatarPath = imagePath;
                    await context.SaveChangesAsync();

                    return Results.Ok(new { imagePath, message = "Avatar uploaded successfully" });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while uploading the avatar");
                }
            });

            // Endpoint to remove user avatar
            userAvatarGroup.MapDelete("/remove", async (
                IImageUploadService imageUploadService,
                QuizContext context,
                ClaimsPrincipal user) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var dbUser = await context.Users.FindAsync(userId);
                if (dbUser == null)
                    return Results.NotFound("User not found");

                if (!string.IsNullOrEmpty(dbUser.AvatarPath))
                {
                    imageUploadService.DeleteImage(dbUser.AvatarPath);
                    dbUser.AvatarPath = null;
                    await context.SaveChangesAsync();
                }

                return Results.Ok(new { message = "Avatar removed successfully" });
            });

            // Endpoint to get user avatar by user id (for displaying avatars)
            userAvatarGroup.MapGet("/user/{userId:int}", async (int userId, QuizContext context) =>
            {
                var dbUser = await context.Users.FindAsync(userId);
                if (dbUser == null)
                    return Results.NotFound("User not found");

                return Results.Ok(new { imagePath = dbUser.AvatarPath });
            });

            return app;
        }
    }
}