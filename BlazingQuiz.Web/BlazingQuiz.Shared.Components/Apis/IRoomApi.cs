using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Shared.Components.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IRoomApi
    {
        [Post("/api/rooms")]
        Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto);

        [Get("/api/rooms")]
        Task<RoomDto[]> GetRoomsAsync();

        [Get("/api/rooms/{id}")]
        Task<RoomDto> GetRoomByIdAsync(Guid id);

        [Get("/api/rooms/code/{code}")]
        Task<RoomDto> GetRoomByCodeAsync(string code);

        [Post("/api/rooms/join")]
        Task<RoomDto> JoinRoomAsync(JoinRoomDto joinRoomDto);

        [Get("/api/rooms/{roomId}/participants")]
        Task<RoomParticipantDto[]> GetRoomParticipantsAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/ready")]
        Task<ApiResponse<object>> SetReadyStatusAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/not-ready")]
        Task<ApiResponse<object>> SetNotReadyStatusAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/start")]
        Task<ApiResponse<object>> StartRoomAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/end")]
        Task<ApiResponse<object>> EndRoomAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/start-quiz")]
        Task<ApiResponse<object>> StartQuizAsync(Guid roomId);

        [Post("/api/rooms/{roomId}/answers")]
        Task<ApiResponse<object>> SubmitAnswerAsync(RoomAnswerDto answerDto, Guid roomId);

        [Get("/api/rooms/{roomId}/answers")]
        Task<RoomAnswerDto[]> GetRoomAnswersAsync(Guid roomId);

        [Get("/api/rooms/{roomId}/answers/user/{userId}")]
        Task<RoomAnswerDto[]> GetRoomAnswersForUserAsync(Guid roomId, int userId);

        [Post("/api/rooms/{roomId}/submit")]
        Task<ApiResponse<object>> SubmitQuizToRoomAsync([Body] SubmitQuizToRoomRequest request, Guid roomId);

        [Get("/api/rooms/{roomId}/submission-status")]
        Task<RoomParticipantDto[]> GetSubmissionStatusAsync(Guid roomId);

        [Get("/api/rooms/admin")]
        Task<List<RoomDto>> GetRoomsForAdminAsync();

        [Delete("/api/rooms/{roomId}")]
        Task<ApiResponse<object>> DeleteRoomAsync(Guid roomId);

        [Delete("/api/rooms/{roomId}/participants/{userId}")]
        Task<ApiResponse<object>> RemoveParticipantAsync(Guid roomId, int userId);

        [Get("/api/rooms/{roomId}/leaderboard")]
        Task<List<QuizRoomLeaderboardEntryDto>> GetRoomLeaderboardAsync(Guid roomId);

        [Get("/api/rooms/history")]
        Task<List<RoomHistoryDto>> GetRoomHistoryAsync();
    }
}