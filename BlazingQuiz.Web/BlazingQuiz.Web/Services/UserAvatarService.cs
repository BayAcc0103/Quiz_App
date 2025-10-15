using BlazingQuiz.Shared.DTOs;
using System.Net.Http.Headers;
using BlazingQuiz.Web.Auth;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazingQuiz.Web.Services
{
    public class UserAvatarService
    {
        private readonly HttpClient _httpClient;
        private readonly QuizAuthStateProvider _authStateProvider;

        private const string ApiServer = "https://localhost:7048"; 

        public UserAvatarService(HttpClient httpClient, QuizAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<QuizApiResponse> UploadAvatarAsync(IBrowserFile file)
        {
            try
            {
                // Create multipart form data
                using var formData = new MultipartFormDataContent();
                
                // Create file content
                var fileContent = new StreamContent(file.OpenReadStream(5 * 1024 * 1024)); // 5MB limit
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "avatar", // Parameter name expected by the API
                    FileName = file.Name
                };
                
                formData.Add(fileContent);

                // Set the authorization header
                var token = _authStateProvider.User?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync("/api/user-avatars/upload", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return QuizApiResponse.Success();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return QuizApiResponse.Failure($"Failed to upload avatar. Status: {response.StatusCode}. Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure($"Exception: {ex.Message}");
            }
        }

        public async Task<QuizApiResponse> RemoveAvatarAsync()
        {
            try
            {
                var token = _authStateProvider.User?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.DeleteAsync("/api/user-avatars/remove");

                if (response.IsSuccessStatusCode)
                {
                    return QuizApiResponse.Success();
                }
                else
                {
                    return QuizApiResponse.Failure("Failed to remove avatar");
                }
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
            }
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            // Construct the full URL to the image on the API server
            // Use the HttpClient's base address that's configured in Program.cs
            return $"{ApiServer}/{imagePath}";
        }
    }
}