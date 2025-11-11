using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class PasswordResetService
    {
        private readonly QuizContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public PasswordResetService(QuizContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<QuizApiResponse> SendResetCodeAsync(ForgotPasswordDto dto, OtpService otpService)
        {
            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                // Return success even if email doesn't exist to prevent email enumeration attacks
                return QuizApiResponse.Success();
            }

            // Additional checks can be added here if needed
            if (!user.IsApproved)
            {
                return QuizApiResponse.Failure("Account is not activated. Contact administrators.");
            }

            // Send OTP via email
            var otpSent = await otpService.SendOtpAsync(dto.Email);
            if (!otpSent)
            {
                return QuizApiResponse.Failure("Failed to send OTP. Please try again later.");
            }

            return QuizApiResponse.Success();
        }

        public async Task<QuizApiResponse> ResetPasswordAsync(ResetPasswordDto dto, OtpService otpService)
        {
            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                return QuizApiResponse.Failure("Invalid email or OTP.");
            }

            // Check if this email was recently verified through the VerifyOtp process
            bool isEmailVerified = otpService.IsEmailVerifiedForReset(dto.Email);
            
            // If not already verified, validate the OTP (which will consume it)
            if (!isEmailVerified)
            {
                var otpValidated = otpService.ValidateOtp(dto.Email, dto.Otp);
                if (!otpValidated)
                {
                    return QuizApiResponse.Failure("Invalid or expired OTP.");
                }
            }
            else
            {
                // If email was already verified via VerifyOtp, we don't need to validate the OTP again
                // but we should remove the verification flag after using it
                otpService.RemoveEmailVerification(dto.Email);
            }

            // Update password only if new passwords are provided
            if (!string.IsNullOrEmpty(dto.NewPassword) && !string.IsNullOrEmpty(dto.ConfirmNewPassword))
            {
                if (dto.NewPassword != dto.ConfirmNewPassword)
                {
                    return QuizApiResponse.Failure("Passwords do not match.");
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
                await _context.SaveChangesAsync();
            }

            return QuizApiResponse.Success();
        }

        public async Task<QuizApiResponse> VerifyOtpAsync(VerifyOtpDto dto, OtpService otpService)
        {
            // Validate OTP
            if (string.IsNullOrWhiteSpace(dto.Otp))
            {
                return QuizApiResponse.Failure("OTP is required.");
            }

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                return QuizApiResponse.Failure("Invalid email or OTP.");
            }

            // Validate OTP using the OTP service (consumes the OTP to prevent reuse)
            var otpValidated = otpService.ValidateOtp(dto.Email, dto.Otp);
            if (!otpValidated)
            {
                return QuizApiResponse.Failure("Invalid or expired OTP.");
            }

            // Mark the email as verified for password reset
            otpService.MarkEmailAsVerified(dto.Email);

            // OTP is valid and email is now verified, return success
            return QuizApiResponse.Success();
        }
    }
}