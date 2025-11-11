using BlazingQuiz.Api.Data.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace BlazingQuiz.Api.Services
{
    public class OtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly GmailOtpService _gmailOtpService;

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger, GmailOtpService gmailOtpService)
        {
            _cache = cache;
            _logger = logger;
            _gmailOtpService = gmailOtpService;
        }

        public string GenerateVerificationToken()
        {
            // Generate a random token for verification
            var random = new Random();
            var token = "";
            for (int i = 0; i < 32; i++)
            {
                token += random.Next(0, 16).ToString("x"); // Hex character
            }
            return token;
        }

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }

        public async Task<bool> SendOtpAsync(string email)
        {
            var otp = GenerateOtp();
            
            // Cache the OTP with expiration (e.g., 10 minutes)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // OTP expires in 10 minutes
            };
            
            _cache.Set($"otp_{email}", otp, cacheOptions);

            // Prepare and send the email via Gmail API
            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>You have requested to reset your password. Please use the following 6-digit code to reset your password:</p>
                    <h3 style=""font-size: 24px; color: #007bff;"">{otp}</h3>
                    <p><strong>This code will expire in 10 minutes.</strong></p>
                    <p>If you did not request this, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>The Blazing Quiz Team</p>
                </body>
                </html>";

            return await _gmailOtpService.SendEmailAsync(email, subject, body);
        }

        public bool ValidateOtp(string email, string otp)
        {
            var cachedOtp = _cache.Get<string>($"otp_{email}");
            
            if (cachedOtp == null)
            {
                _logger.LogWarning($"No OTP found for email: {email}");
                return false;
            }

            var isValid = string.Equals(cachedOtp, otp, StringComparison.OrdinalIgnoreCase);
            
            if (isValid)
            {
                // Remove the OTP after successful validation to prevent reuse
                _cache.Remove($"otp_{email}");
            }

            return isValid;
        }

        public string IssueVerificationToken(string email)
        {
            var token = GenerateVerificationToken();
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // Token expires in 15 minutes
            };

            // Store email against the token for later validation
            _cache.Set($"ver_token_{token}", email, cacheOptions);
            return token;
        }

        public void MarkEmailAsVerified(string email)
        {
            // Mark this email as verified for password reset for the next 10 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Verification valid for 10 mins
            };

            _cache.Set($"verified_{email}", true, cacheOptions);
        }

        public bool IsEmailVerifiedForReset(string email)
        {
            return _cache.Get<bool>($"verified_{email}");
        }

        public void RemoveEmailVerification(string email)
        {
            _cache.Remove($"verified_{email}");
        }
    }
}