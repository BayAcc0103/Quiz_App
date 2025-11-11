using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System.Text;

namespace BlazingQuiz.Api.Services
{
    public class GmailOtpService
    {
        private readonly IConfiguration _configuration;

        public GmailOtpService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var refreshToken = _configuration["GoogleOAuth:RefreshToken"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Google OAuth configuration is missing. Please set ClientId, ClientSecret, and RefreshToken in appsettings.json");
            }

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to refresh access token: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonDocument.Parse(responseContent);
            
            return tokenResponse.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                
                // Create Google credential with access token
                var credential = GoogleCredential.FromAccessToken(accessToken);
                
                // Create Gmail service
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "BlazingQuiz"
                });

                // Create email message
                var message = new Message();
                message.Raw = EncodeEmail(_configuration["GoogleOAuth:Email"], toEmail, subject, body);

                // Send the email
                await service.Users.Messages.Send(message, "me").ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email via Gmail API: {ex.Message}");
                return false;
            }
        }

        private string EncodeEmail(string from, string to, string subject, string body)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("From: {0}\r\n", from);
            builder.AppendFormat("To: {0}\r\n", to);
            builder.AppendFormat("Subject: {0}\r\n", subject);
            builder.Append("Content-Type: text/html; charset=utf-8\r\n");
            builder.Append("\r\n");
            builder.Append(body);

            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(builder.ToString()))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            return encoded;
        }
    }
}