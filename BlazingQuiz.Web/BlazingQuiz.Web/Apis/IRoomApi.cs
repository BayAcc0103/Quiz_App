using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
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
        
        [Post("/api/rooms/{roomId}/start-quiz")]
        Task<ApiResponse<object>> StartQuizAsync(Guid roomId);
        
        [Post("/api/rooms/{roomId}/answers")]
        Task<ApiResponse<object>> SubmitAnswerAsync(RoomAnswerDto answerDto, Guid roomId);
        
        [Get("/api/rooms/{roomId}/answers")]
        Task<RoomAnswerDto[]> GetRoomAnswersAsync(Guid roomId);
        
        [Get("/api/rooms/{roomId}/answers/user/{userId}")]
        Task<RoomAnswerDto[]> GetRoomAnswersForUserAsync(Guid roomId, int userId);
    }
}