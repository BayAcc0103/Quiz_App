using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Shared.Components.Apis
{
    public interface IPublicQuizApi
    {
        [Get("/api/student/available-quizes")]
        Task<QuizListDto[]> GetActiveQuizesAsync(int categoryId);

        [Get("/api/student/quiz/{quizId}/details")]
        Task<QuizApiResponse<QuizDetailsDto>> GetQuizDetailsAsync(Guid quizId);

        [Get("/api/student/quiz/{quizId}/all-feedback")]
        Task<QuizApiResponse<QuizAllFeedbackDto>> GetAllFeedbackAsync(Guid quizId);
    }
}