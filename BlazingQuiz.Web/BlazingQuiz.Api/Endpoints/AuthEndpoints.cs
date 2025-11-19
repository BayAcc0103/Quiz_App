using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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

            // 1) Endpoint cho FE gọi để bắt đầu đăng nhập với Google
            app.MapGet("/authorize/google-login-url", (HttpContext context) =>
            {
                var apiHost = $"{context.Request.Scheme}://{context.Request.Host}";
                var loginUrl = $"{apiHost}/authorize/google-login";
                return Results.Ok(new { url = loginUrl });
            });

            // 2) Endpoint /authorize/google-login: gọi Challenge("Google")
            app.MapGet("/authorize/google-login", async (HttpContext context) =>
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = "/authorize/login-callback" // nơi Google middleware sẽ redirect về sau khi login xong
                };

                return Results.Challenge(props, new[] { "Google" });
            });
            // This endpoint handles the Google OAuth callback automatically
            // When Google redirects back to our callback URL, the middleware processes it
            // and the user is authenticated - we need to return the JWT to the frontend
            app.MapGet("/authorize/login-callback", async (HttpContext context, GoogleAuthService googleAuthService, ILoggerFactory loggerFactory) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GoogleCallback");
                var frontendUrl = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Jwt:Audience") ?? "https://localhost:7194";
                // Get frontend URL once for use in all redirect scenarios

                // Authenticate with Google OAuth provider - this is handled by the middleware
                var authenticateResult = await context.AuthenticateAsync("Cookies");

                if (!authenticateResult.Succeeded)
                {
                    logger.LogError("Google authenticate failed. Error: {Error}; Failure: {Failure}",
                        authenticateResult.Failure?.Message,
                        authenticateResult.Failure?.ToString());
                    // Handle authentication failure by redirecting to frontend with error
                    var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
                    return Results.Redirect(redirectUrl);
                }

                // Extract user information from the Google authentication
                var googleIdClaim = authenticateResult.Principal.FindFirst("sub"); // Google's user ID
                var emailClaim = authenticateResult.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") ??
                                authenticateResult.Principal.FindFirst("email");
                var nameClaim = authenticateResult.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") ??
                               authenticateResult.Principal.FindFirst("name");
                var pictureClaim = authenticateResult.Principal.FindFirst("picture"); // Google profile picture

                if (emailClaim == null || googleIdClaim == null)
                {
                    // Handle missing email or googleId by redirecting to frontend with error
                    var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
                    return Results.Redirect(redirectUrl);
                }

                var googleId = googleIdClaim.Value;
                var email = emailClaim.Value;
                var name = nameClaim?.Value ?? "Google User";
                var picture = pictureClaim?.Value;

                // Create or retrieve user and generate JWT, linking Google account to existing user if needed
                var authResult = await googleAuthService.LoginWithGoogleAsync(googleId, email, name, picture);

                if (authResult.HasError)
                {
                    // Handle authentication error by redirecting to frontend with error
                    var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
                    return Results.Redirect(redirectUrl);
                }

                // Get the JWT token from the result
                var jwtToken = authResult.User?.Token;
                var role = authResult.User?.Role;

                if (string.IsNullOrEmpty(jwtToken))
                {
                    // Handle JWT generation error by redirecting to frontend with error
                    var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
                    return Results.Redirect(redirectUrl);
                }

                // Redirect back to frontend with JWT in URL fragment (this is how SPAs typically handle auth codes/jwt)
                var successRedirectUrl = $"{frontendUrl}/auth/google-callback#token={System.Uri.EscapeDataString(jwtToken)}&role={System.Uri.EscapeDataString(role ?? "Student")}";

                return Results.Redirect(successRedirectUrl);
            });

            return app;
        }
    }
}