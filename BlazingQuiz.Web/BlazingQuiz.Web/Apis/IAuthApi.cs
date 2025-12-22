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
        
        [Post("/api/auth/send-reset-code")]
        Task<QuizApiResponse> SendResetCodeAsync(ForgotPasswordDto dto);
        
        [Post("/api/auth/reset-password")]
        Task<QuizApiResponse> ResetPasswordAsync(ResetPasswordDto dto);

        [Post("/api/auth/verify-otp")]
        Task<QuizApiResponse> VerifyOtpAsync(VerifyOtpDto dto);

        [Post("/api/auth/google-login")]
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
    }
}
