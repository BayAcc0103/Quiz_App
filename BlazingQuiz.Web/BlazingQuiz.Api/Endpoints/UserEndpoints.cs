using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

            group.MapGet("", async (UserApprovedFilter approveType, int startIndex, int pageSize, UserService userService) =>
            {
                //var approvedFilter = Enum.Parse<UserApprovedFilter>(filter);
                return Results.Ok(await userService.GetUserAsync(approveType, startIndex, pageSize));
            });
            group.MapPatch("{userId:int}/toggle-status", async (int userId, UserService userService) =>
            {
                await userService.ToggleUserApprovedStatusAsync(userId);
                return Results.Ok();
            });

            return app;
        }
    }
}
