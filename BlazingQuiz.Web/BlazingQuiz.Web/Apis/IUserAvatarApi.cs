using Refit;

namespace BlazingQuiz.Web.Apis
{
    public interface IUserAvatarApi
    {
        [Multipart]
        [Post("/api/user-avatars/upload")]
        Task<ApiResponse<object>> UploadAvatarAsync(MultipartFormDataContent content);

        [Delete("/api/user-avatars/remove")]
        Task<ApiResponse<object>> RemoveAvatarAsync();

        [Get("/api/user-avatars/user/{userId}")]
        Task<ApiResponse<object>> GetUserAvatarAsync(int userId);
    }
}