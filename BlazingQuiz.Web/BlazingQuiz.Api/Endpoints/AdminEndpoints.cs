using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;

namespace BlazingQuiz.Api.Endpoints
{
    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var adminGroup = app.MapGroup("/api/admin")
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

            adminGroup.MapGet("/home-data", async (AdminService userService) =>
                Results.Ok(await userService.GetAdminHomeDataAsync()));

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

            return app;
        }
    }
}
