using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;

namespace BlazingQuiz.Api.Endpoints
{
    public static class CategoryEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categoriesGroup = app.MapGroup("/api/categories").RequireAuthorization();
            categoriesGroup.MapGet("", async (CategoryService categoryService) => 
                Results.Ok(await categoryService.GetCategoriesAsync()));
            categoriesGroup.MapPost("", async (CategoryDto dto, CategoryService categoryService) =>
                Results.Ok(await categoryService.SaveCategoryAsync(dto)))
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            return app;
        }
    }
}
