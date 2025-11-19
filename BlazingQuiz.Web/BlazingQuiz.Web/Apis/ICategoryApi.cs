using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers ("Authorization: Bearer")]
    public interface ICategoryApi
    {
        [Post("/api/categories")]
        Task<QuizApiResponse> SaveCategoryAsync(CategoryDto categoryDto);
        [Get("/api/categories")]
        Task<CategoryDto[]> GetCategoriesAsync();
        [Delete("/api/categories/{id}")]
        Task<QuizApiResponse> DeleteCategoryAsync(int id);
    }
}
