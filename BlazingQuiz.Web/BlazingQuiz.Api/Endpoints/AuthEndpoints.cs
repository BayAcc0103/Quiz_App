using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/login", async (LoginDto dto, AuthService authService) =>
            Results.Ok(await authService.LoginAsync(dto)));

            app.MapPost("/api/auth/register", async (RegisterDto dto, AuthService authService) =>
            Results.Ok(await authService.RegisterAsync(dto)));

            app.MapGet("/api/auth/profile", [Authorize] async (ClaimsPrincipal user, QuizContext context) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var dbUser = await context.Users.FindAsync(userId);
                if (dbUser == null)
                {
                    return Results.NotFound();
                }
                
                var userDto = new UserDto(dbUser.Id, dbUser.Name, dbUser.Email, dbUser.Phone, dbUser.IsApproved, dbUser.AvatarPath);
                return Results.Ok(userDto);
            });

            app.MapPut("/api/auth/profile", [Authorize] async (ClaimsPrincipal user, QuizContext context, UserDto userDto) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var dbUser = await context.Users.FindAsync(userId);
                if (dbUser == null)
                {
                    return Results.NotFound();
                }
                
                // Update user properties
                dbUser.Name = userDto.Name;
                dbUser.Email = userDto.Email;
                dbUser.Phone = userDto.Phone;
                
                await context.SaveChangesAsync();
                return Results.Ok();
            });

            app.MapPut("/api/auth/changepassword", [Authorize] async (ClaimsPrincipal user, QuizContext context, IPasswordHasher<User> passwordHasher, ChangePasswordDto dto) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var dbUser = await context.Users.FindAsync(userId);
                if (dbUser == null)
                {
                    return Results.NotFound();
                }
                
                // Verify current password
                var passwordResult = passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, dto.CurrentPassword);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    return Results.BadRequest("Current password is incorrect.");
                }
                
                // Set new password
                dbUser.PasswordHash = passwordHasher.HashPassword(dbUser, dto.NewPassword);
                await context.SaveChangesAsync();
                return Results.Ok();
            });

            return app;       
        }
    }
}
