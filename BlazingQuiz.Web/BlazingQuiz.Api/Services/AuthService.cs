using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazingQuiz.Api.Services
{
    public class AuthService
    {
        private readonly QuizContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(QuizContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == dto.Username);
            if (user == null)
            {
                //Invalid username
                return new AuthResponseDto(default, "Invalid username");
            }
            if(!user.IsApproved)
            {
                //User is not approved yet
                return new AuthResponseDto(default, "User is not approved yet");
            }
            if (user.Role != dto.Role.ToString())
            {
                // Role mismatch
                return new AuthResponseDto(default, $"Login failed. User is not a {dto.Role}.");
            }
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                //Incorrect password
                return new AuthResponseDto(default, "Incorrect password");
            }
            // Gen JWT Token
            var jwt = GenerateJwtToken(user);
            //var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Role, jwt);
            var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Email, user.Role, jwt, user.AvatarPath)
            {
                FullName = user.Name
            };
            return new AuthResponseDto(loggedInUser);
        }
        public async Task<QuizApiResponse> RegisterAsync(RegisterDto dto)
        {
            if(await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return QuizApiResponse.Failure("Email already exists.Try logging in");
            }
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role.ToString(),
                IsApproved = dto.Role.ToString().ToLower() == "student"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _context.Users.Add(user);
            try 
            {                
                await _context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error during registration: {ex.Message}");
                return QuizApiResponse.Failure(ex.Message);
            }
        }
        private string GenerateJwtToken (User user)
        {
            Claim[] claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                ];
            var secretKey = _configuration.GetValue<string>("Jwt:Secret");
            var symmetricKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var signingCred = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireInMinutes")),
                signingCredentials: signingCred);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;
        }

        public async Task<QuizApiResponse> SendResetCodeAsync(ForgotPasswordDto dto, OtpService otpService, EmailService emailService)
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
            // Validate OTP first
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

            // Validate OTP using the OTP service
            var otpValidated = otpService.ValidateOtp(dto.Email, dto.Otp);
            if (!otpValidated)
            {
                return QuizApiResponse.Failure("Invalid or expired OTP.");
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            return QuizApiResponse.Success();
        }
    }
}
