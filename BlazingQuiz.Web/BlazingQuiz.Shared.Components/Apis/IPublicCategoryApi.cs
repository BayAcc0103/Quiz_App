using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Shared.Components.Apis
{
    public interface IPublicCategoryApi
    {
        [Get("/api/categories")]
        Task<CategoryDto[]> GetCategoriesAsync();
    }
}