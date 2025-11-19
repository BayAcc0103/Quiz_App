using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;

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
            categoriesGroup.MapPost("", async (CategoryDto dto, CategoryService categoryService) =>
                Results.Ok(await categoryService.SaveCategoryAsync(dto)))
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            categoriesGroup.MapDelete("/{id:int}", async (int id, CategoryService categoryService) =>
                Results.Ok(await categoryService.DeleteCategoryAsync(id)))
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            return app;
        }
    }
}
