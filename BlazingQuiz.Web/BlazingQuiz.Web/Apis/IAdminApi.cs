using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IAdminApi
    {
        [Get("/api/admin/users")]
        Task<PageResult<UserDto>> GetUsersAsync(UserApprovedFilter approveType, int startIndex, int pageSize);

        [Patch("/api/admin/users/{userId}/toggle-status")]
        Task ToggleUserApprovedStatusAsync(int userId);

        [Get("/api/admin/home-data")]
        Task<AdminHomeDataDto> GetAdminHomeDataAsync();

        [Get("/api/admin/quizes/{quizId}/students")]
        Task<AdminQuizStudentListDto> GetQuizStudentsAsync(Guid quizId, int startIndex, int pageSize, bool fetchQuizInfo);
    }
}
