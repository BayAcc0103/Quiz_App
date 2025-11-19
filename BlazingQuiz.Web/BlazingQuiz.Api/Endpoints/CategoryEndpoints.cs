using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class CategoryEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            // Allow anonymous access to get categories
            app.MapGet("/api/categories", async (CategoryService categoryService) =>
                Results.Ok(await categoryService.GetCategoriesAsync()))
                .AllowAnonymous();

            var categoriesGroup = app.MapGroup("/api/categories").RequireAuthorization();
            categoriesGroup.MapPost("", async (CategoryDto dto, CategoryService categoryService, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? createdByUserId = userId != null ? int.Parse(userId) : (int?)null;
                return Results.Ok(await categoryService.SaveCategoryAsync(dto, createdByUserId));
            })
            .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            categoriesGroup.MapGet("/bycreator", async (CategoryService categoryService, HttpContext httpContext) =>
            {
                // Get user ID from claims - it's stored as NameIdentifier
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out var userId))
                {
                    var categories = await categoryService.GetCategoriesByCreatorAsync(userId);
                    return Results.Ok(categories);
                }
                return Results.BadRequest("Invalid user ID");
            })
            .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Teacher)));
            categoriesGroup.MapDelete("/{id:int}", async (int id, CategoryService categoryService) =>
                Results.Ok(await categoryService.DeleteCategoryAsync(id)))
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            return app;
        }
    }
}
