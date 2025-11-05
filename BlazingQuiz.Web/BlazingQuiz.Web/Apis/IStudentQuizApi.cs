using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IStudentQuizApi
    {
        [Get("/api/student/available-quizes")]
        Task<QuizListDto[]> GetActiveQuizesAsync(int categoryId);

        [Get("/api/student/quiz/{studentQuizId}/result")]
        Task<QuizApiResponse<QuizResultDto>> GetQuizResultAsync(int studentQuizId);

        [Get("/api/student/my-quizes")]
        Task<PageResult<StudentQuizDto>> GetStudentQuizesAsync(int startIndex, int pageSize);

        [Post("/api/student/quiz/{quizId}/start")]
        Task<QuizApiResponse<int>> StartQuizAsync(Guid quizId);

        [Get("/api/student/quiz/{studentQuizId}/next-question")]
        Task<QuizApiResponse<QuestionDto?>> GetNextQuestionForQuizAsync(int studentQuizId);

        [Post("/api/student/quiz/{studentQuizId}/save-response")]      
        Task<QuizApiResponse> SaveQuestionResponseAsync(int studentQuizId, StudentQuizQuestionResponseDto dto);

        [Post("/api/student/quiz/{studentQuizId}/submit")]
        Task<QuizApiResponse> SubmitQuizAsync(int studentQuizId);

        [Post("/api/student/quiz/{studentQuizId}/exit")]
        Task<QuizApiResponse> ExitQuizAsync(int studentQuizId);

        [Post("/api/student/quiz/{studentQuizId}/auto-submit")]
        Task<QuizApiResponse> AutoSubmitQuizAsync(int studentQuizId);
        
        [Get("/api/student/quiz/{studentQuizId}/all-questions")]
        Task<QuizApiResponse<IEnumerable<QuestionDto>>> GetAllQuestionsForQuizAsync(int studentQuizId);

        [Post("/api/student/quiz/{studentQuizId}/rate-and-comment")]
        Task<QuizApiResponse> SaveRatingAndCommentAsync(int studentQuizId, QuizRatingCommentDto dto);
        
        [Get("/api/student/quiz/{quizId}/details")]
        Task<QuizApiResponse<QuizDetailsDto>> GetQuizDetailsAsync(Guid quizId);
        
        [Get("/api/student/quiz/{quizId}/all-feedback")]
        Task<QuizApiResponse<QuizAllFeedbackDto>> GetAllFeedbackAsync(Guid quizId);
        
        [Get("/api/student/quizzes/quiz/{quizId}")]
        Task<StudentQuizDto[]> GetStudentQuizzesByQuizIdAsync(Guid quizId);
        
        [Get("/api/student/quiz/{studentQuizId}/responses")]
        Task<StudentQuizQuestionResultDto[]> GetStudentQuizQuestionResponsesAsync(int studentQuizId);
    }
}