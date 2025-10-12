using BlazingQuiz.Shared.DTOs;
using Refit;

namespace BlazingQuiz.Web.Apis
{
    [Headers("Authorization: Bearer")]
    public interface IAuthApi
    {
        [Post("/api/auth/login")]
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        [Post("/api/auth/register")]
        Task<QuizApiResponse> RegisterAsync(RegisterDto dto);
        
        [Get("/api/auth/profile")]
        Task<UserDto> GetProfileAsync();
        
        [Put("/api/auth/profile")]
        Task UpdateProfileAsync(UserDto userDto);
        
        [Put("/api/auth/changepassword")]
        Task ChangePasswordAsync(ChangePasswordDto dto);
    }
}
