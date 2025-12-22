using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            app.MapPost("/api/auth/send-reset-code", async (ForgotPasswordDto dto, PasswordResetService passwordResetService, OtpService otpService) =>
            {
                var result = await passwordResetService.SendResetCodeAsync(dto, otpService);
                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            });

            app.MapPost("/api/auth/reset-password", async (ResetPasswordDto dto, PasswordResetService passwordResetService, OtpService otpService) =>
            {
                var result = await passwordResetService.ResetPasswordAsync(dto, otpService);
                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            });

            app.MapPost("/api/auth/verify-otp", async (VerifyOtpDto dto, PasswordResetService passwordResetService, OtpService otpService) =>
            {
                var result = await passwordResetService.VerifyOtpAsync(dto, otpService);
                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            });

            app.MapPost("/api/auth/google-login", async (GoogleLoginDto dto, GoogleAuthService googleAuthService, AuthService authService, QuizContext context, IPasswordHasher<User> passwordHasher) =>
            {
                try
                {
                    // Verify the Google ID token
                    var googlePayload = await googleAuthService.VerifyGoogleTokenAsync(dto.GoogleIdToken);

                    // Find or create user based on Google email
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == googlePayload.Email);

                    if (user == null)
                    {
                        // Create new user
                        user = new User
                        {
                            Name = googlePayload.Name ?? googlePayload.GivenName + " " + googlePayload.FamilyName,
                            Email = googlePayload.Email,
                            Phone = "",
                            Role = dto.Role,
                            IsApproved = true, // Google authenticated users are automatically approved
                            AvatarPath = googlePayload.Picture // Store Google profile picture URL
                        };
                        user.PasswordHash = passwordHasher.HashPassword(user, Guid.NewGuid().ToString()); // Generate a random password
                        context.Users.Add(user);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        // Update user info if needed
                        user.Name = googlePayload.Name ?? googlePayload.GivenName + " " + googlePayload.FamilyName;
                        user.AvatarPath = googlePayload.Picture;
                        user.Role = dto.Role; // Update role if needed
                        await context.SaveChangesAsync();
                    }

                    // Generate JWT token
                    var jwt = authService.GenerateJwtToken(user);

                    var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Email, user.Role, jwt, user.AvatarPath)
                    {
                        FullName = user.Name
                    };

                    return Results.Ok(new AuthResponseDto(loggedInUser));
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Results.Unauthorized();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred during Google login: {ex.Message}");
                }
            });

            return app;
        }
    }
}