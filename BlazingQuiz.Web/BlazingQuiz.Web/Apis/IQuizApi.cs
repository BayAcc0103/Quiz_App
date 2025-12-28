using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
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
        
        [Delete("/api/quizes/ratings/{ratingId}")]
        Task<QuizApiResponse> DeleteRatingAsync(int ratingId);
        
        [Delete("/api/quizes/comments/{commentId}")]
        Task<QuizApiResponse> DeleteCommentAsync(int commentId);

        [Delete("/api/quizes/options/{optionId}")]
        Task<QuizApiResponse> DeleteOptionAsync(int optionId);

        [Delete("/api/quizes/questions/{questionId}")]
        Task<QuizApiResponse> DeleteQuestionAsync(int questionId);

        [Delete("/api/quizes/{quizId}")]
        Task<QuizApiResponse> DeleteQuizAsync(Guid quizId);

        [Get("/api/quizes/{quizId}")]
        Task<ApiResponse<QuizDetailsDto>> GetQuizByIdAsync(string quizId);

        [Get("/api/quizes/{quizId}/students")]
        Task<TeacherQuizStudentListDto> GetQuizStudentsAsync(Guid quizId, int startIndex, int pageSize, bool fetchQuizInfo);

        [Get("/api/questions")]
        Task<QuizApiResponse<QuestionDto[]>> GetQuestionsAsync();

        [Get("/api/questions/created-by/{userId}")]
        Task<QuizApiResponse<QuestionDto[]>> GetQuestionsCreatedByAsync(int userId);

        [Get("/api/questions/{questionId}")]
        Task<QuizApiResponse<QuestionDto>> GetQuestionByIdAsync(int questionId);

        [Post("/api/questions")]
        Task<QuizApiResponse> SaveQuestionAsync(QuestionDto question);

        [Put("/api/questions/{questionId}")]
        Task<QuizApiResponse> UpdateQuestionAsync(int questionId, QuestionDto question);

        [Post("/api/quizes/{quizId}/ban")]
        Task<QuizApiResponse> BanQuizAsync(Guid quizId);

        [Post("/api/quizes/{quizId}/unban")]
        Task<QuizApiResponse> UnbanQuizAsync(Guid quizId);
    }
}
