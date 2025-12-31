using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var adminGroup = app.MapGroup("/api/admin")
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

            adminGroup.MapGet("/home-data", async (AdminService userService, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var data = await userService.GetAdminHomeDataAsync(userId);
                return Results.Ok(data);
            });

            adminGroup.MapGet("/quizes/{quizId:guid}/students", async (Guid quizId, int startIndex, int pageSize, bool fetchQuizInfo, AdminService userService) =>
                Results.Ok(await userService.GetQuizStudentsAsync(quizId, startIndex, pageSize, fetchQuizInfo)));


            var group = adminGroup.MapGroup("/users");

            group.MapGet("", async (UserApprovedFilter approveType, int startIndex, int pageSize, AdminService userService) =>
            {
                //var approvedFilter = Enum.Parse<UserApprovedFilter>(filter);
                return Results.Ok(await userService.GetUserAsync(approveType, startIndex, pageSize));
            });
            group.MapPatch("{userId:int}/toggle-status", async (int userId, AdminService userService) =>
            {
                await userService.ToggleUserApprovedStatusAsync(userId);
                return Results.Ok();
            });

            group.MapPost("/create-user", async (RegisterDto dto, AdminService userService) =>
            {
                var result = await userService.CreateUserAsync(dto);
                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            });

            // Add notification endpoints
            adminGroup.MapPost("/notifications/{notificationId:int}/mark-as-read", async (int notificationId, NotificationService notificationService) =>
            {
                await notificationService.MarkAsReadAsync(notificationId);
                return Results.Ok();
            });

            adminGroup.MapDelete("/notifications/{notificationId:int}", async (int notificationId, NotificationService notificationService, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest(new { success = false, message = "Invalid user ID" });
                }

                var result = await notificationService.DeleteNotificationAsync(notificationId, userId);
                if (result)
                {
                    return Results.Ok(new { success = true, message = "Notification deleted successfully" });
                }
                else
                {
                    return Results.NotFound(new { success = false, message = "Notification not found" });
                }
            });

            return app;
        }
    }
}
