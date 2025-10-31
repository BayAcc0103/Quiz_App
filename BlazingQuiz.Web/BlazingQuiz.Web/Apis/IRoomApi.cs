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
    }
}