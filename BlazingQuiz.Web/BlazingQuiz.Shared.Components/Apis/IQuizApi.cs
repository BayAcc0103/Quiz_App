using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Shared.Components.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IQuizApi
    {
        [Post ("/api/quizes")]
        Task<QuizApiResponse> SaveQuizAsync(QuizSaveDto saveDto);
        [Get("/api/quizes")]
        Task<QuizListDto[]> GetQuizesAsync();
        [Get("/api/quizes/{quizId}/questions")]
        Task<QuestionDto[]> GetQuizQuestionsAsync(Guid quizId);

        [Get("/api/quizes/{quizId}")]
        Task<QuizSaveDto?> GetQuizToEditAsync(Guid quizId);
        
        [Get("/api/quizes/{quizId}/feedback")]
        Task<QuizApiResponse<TeacherQuizFeedbackDto>> GetQuizFeedbackAsync(Guid quizId);
        
        [Delete("/api/quizes/feedback/{feedbackId}")]
        Task<QuizApiResponse> DeleteQuizFeedbackAsync(int feedbackId);
    }
}
