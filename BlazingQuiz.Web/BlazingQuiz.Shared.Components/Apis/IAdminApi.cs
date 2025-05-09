using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IAdminApi
    {
        [Get("/api/users")]
        Task<PageResult<UserDto>> GetUsersAsync(UserApprovedFilter approveType, int startIndex, int pageSize);

        [Patch("/api/users/{userId}/toggle-status")]
        Task ToggleUserApprovedStatusAsync(int userId);

        [Get("/api/admin/home-data")]
        Task<AdminHomeDataDto> GetAdminHomeDataAsync();
    }
}
