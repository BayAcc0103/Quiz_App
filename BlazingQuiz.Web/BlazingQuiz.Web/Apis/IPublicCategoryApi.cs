using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    public interface IPublicCategoryApi
    {
        [Get("/api/categories")]
        Task<CategoryDto[]> GetCategoriesAsync();
    }
}