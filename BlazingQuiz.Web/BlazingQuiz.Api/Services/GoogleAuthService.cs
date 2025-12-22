using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace BlazingQuiz.Api.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["GoogleOAuth:ClientId"];
                
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
                
                // Verify that the token is not expired
                if (payload.ExpirationTimeSeconds.HasValue)
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationTimeSeconds.Value);
                    if (DateTimeOffset.UtcNow > expirationTime)
                    {
                        throw new InvalidOperationException("Google ID token has expired.");
                    }
                }

                return payload;
            }
            catch (InvalidJwtException ex)
            {
                throw new UnauthorizedAccessException($"Invalid Google ID token: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Google token verification failed: {ex.Message}");
            }
        }
    }
}