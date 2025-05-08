using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Api.Endpoints
{
    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/home-data", async (AdminService userService) =>
                Results.Ok(await userService.GetAdminHomeDataAsync()))
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

            var group = app.MapGroup("/api/users").RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

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

            return app;
        }
    }
}
