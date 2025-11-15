using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IRoomQuizApi
    {
        [Post("/api/room-quiz/quiz/{roomId}/start")]
        Task<QuizApiResponse<int>> StartQuizForRoomAsync(Guid roomId);

        [Get("/api/room-quiz/quiz/{studentQuizForRoomId}/next-question")]
        Task<QuizApiResponse<QuestionDto?>> GetNextQuestionForRoomQuizAsync(int studentQuizForRoomId);

        [Post("/api/room-quiz/quiz/{studentQuizForRoomId}/save-response")]
        Task<QuizApiResponse> SaveQuestionResponseForRoomQuizAsync(int studentQuizForRoomId, StudentQuizQuestionResponseDto dto);

        [Post("/api/room-quiz/quiz/{studentQuizForRoomId}/submit")]
        Task<QuizApiResponse> SubmitRoomQuizAsync(int studentQuizForRoomId);

        [Post("/api/room-quiz/quiz/{studentQuizForRoomId}/exit")]
        Task<QuizApiResponse> ExitRoomQuizAsync(int studentQuizForRoomId);

        [Post("/api/room-quiz/quiz/{studentQuizForRoomId}/auto-submit")]
        Task<QuizApiResponse> AutoSubmitRoomQuizAsync(int studentQuizForRoomId);

        [Get("/api/room-quiz/quiz/{studentQuizForRoomId}/result")]
        Task<QuizApiResponse<QuizResultDto>> GetRoomQuizResultAsync(int studentQuizForRoomId);
    }
}