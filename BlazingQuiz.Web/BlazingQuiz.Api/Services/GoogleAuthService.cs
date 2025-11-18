using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text.Json;

namespace BlazingQuiz.Api.Services
{
    public class GoogleAuthService
    {
        private readonly QuizContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(QuizContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Generates a Google login URL for the frontend to redirect to
        /// </summary>
        public string GenerateGoogleLoginUrl(string state, string apiHost = null)
        {
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var scopes = "openid profile email";

            var stateParam = string.IsNullOrEmpty(state) ? Guid.NewGuid().ToString() : state;

            // Use the provided host or default to localhost:7048
            var host = apiHost ?? "localhost:7048";
            var redirectUri = $"https://{host}/authorize/login-callback";

            // Store state in cache or session for validation later
            // In a real app, you'd use a distributed cache with expiration
            var loginUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                          $"client_id={clientId}&" +
                          $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                          $"response_type=code&" +
                          $"scope={Uri.EscapeDataString(scopes)}&" +
                          $"state={Uri.EscapeDataString(stateParam)}&" +
                          $"access_type=offline&" +
                          $"prompt=consent";

            return loginUrl;
        }

        /// <summary>
        /// Processes the Google OAuth callback, exchanges code for tokens, gets user info, and returns JWT
        /// </summary>
        public async Task<AuthResponseDto> ProcessGoogleCallbackAsync(string code, string state, string receivedState)
        {
            // Validate the state parameter for security
            if (state != receivedState)
            {
                return new AuthResponseDto(null, "Invalid state parameter");
            }

            // Exchange the authorization code for access token and ID token
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri
            };

            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return new AuthResponseDto(null, "Failed to exchange code for token");
            }

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

            var accessToken = tokenData.GetProperty("access_token").GetString();
            var idToken = tokenData.GetProperty("id_token").GetString();

            // Use the access token to get user information from Google
            var userInfoResponse = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return new AuthResponseDto(null, "Failed to get user info from Google");
            }

            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

            // Find or create the user in our database
            var existingUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userInfo.Email.ToLower());

            User user;
            if (existingUser != null)
            {
                // User already exists
                user = existingUser;
            }
            else
            {
                // Create new user
                user = new User
                {
                    Name = userInfo.Name ?? userInfo.Email.Split('@')[0],
                    Email = userInfo.Email,
                    Phone = "",
                    Role = UserRole.Student.ToString(), // Default to Student for Google users
                    IsApproved = true, // Google authenticated users are automatically approved
                    PasswordHash = _passwordHasher.HashPassword(new User(), GenerateRandomPassword()) // Generate a random password for OAuth users
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token for our system
            var jwt = GenerateJwtToken(user);
            var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Email, user.Role, jwt, user.AvatarPath)
            {
                FullName = user.Name
            };

            return new AuthResponseDto(loggedInUser);
        }

        public async Task<AuthResponseDto> ProcessGoogleAuthAsync(string code, string state, string returnUrl = null)
        {
            // Verify the state parameter for security if needed
            // In a real application, you would store the state in a secure session and verify it here

            // Exchange the authorization code for access token and ID token
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var clientId = _configuration["GoogleOAuth:ClientId"];
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri
            };

            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return new AuthResponseDto(null, "Failed to exchange code for token");
            }

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

            var accessToken = tokenData.GetProperty("access_token").GetString();
            var idToken = tokenData.GetProperty("id_token").GetString();

            // Use the access token to get user information from Google
            var userInfoResponse = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                return new AuthResponseDto(null, "Failed to get user info from Google");
            }

            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

            // Find or create the user in our database
            var existingUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userInfo.Email.ToLower());

            User user;
            if (existingUser != null)
            {
                // User already exists
                user = existingUser;
            }
            else
            {
                // Create new user
                user = new User
                {
                    Name = userInfo.Name ?? userInfo.Email.Split('@')[0],
                    Email = userInfo.Email,
                    Phone = "",
                    Role = UserRole.Student.ToString(), // Default to Student for Google users
                    IsApproved = true, // Google authenticated users are automatically approved
                    PasswordHash = _passwordHasher.HashPassword(new User(), GenerateRandomPassword()) // Generate a random password for OAuth users
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token for our system
            var jwt = GenerateJwtToken(user);
            var loggedInUser = new LoggedInUser(user.Id, user.Name, user.Email, user.Role, jwt, user.AvatarPath)
            {
                FullName = user.Name
            };

            return new AuthResponseDto(loggedInUser);
        }

        public async Task<AuthResponseDto> LoginWithGoogleAsync(string email, string name)
        {
            // This method is used by the OAuth middleware approach
            // Check if user already exists in the database
            var existingUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (existingUser != null)
            {
                // User exists, generate JWT token
                var jwt = GenerateJwtToken(existingUser);
                var loggedInUser = new LoggedInUser(existingUser.Id, existingUser.Name, existingUser.Email, existingUser.Role, jwt, existingUser.AvatarPath)
                {
                    FullName = existingUser.Name
                };
                return new AuthResponseDto(loggedInUser);
            }
            else
            {
                // Create new user (default as Student role)
                var newUser = new User
                {
                    Name = name,
                    Email = email,
                    Role = UserRole.Student.ToString(), // Default to Student
                    IsApproved = true, // Google authenticated users are automatically approved
                    PasswordHash = _passwordHasher.HashPassword(new User(), GenerateRandomPassword()) // Generate a random password for OAuth users
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Generate JWT token for the new user
                var jwt = GenerateJwtToken(newUser);
                var loggedInUser = new LoggedInUser(newUser.Id, newUser.Name, newUser.Email, newUser.Role, jwt, newUser.AvatarPath)
                {
                    FullName = newUser.Name
                };
                return new AuthResponseDto(loggedInUser);
            }
        }

        private string GenerateJwtToken(User user)
        {
            Claim[] claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
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

        private string GenerateRandomPassword()
        {
            // Generate a random password for OAuth users since they don't need it
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
    }
}