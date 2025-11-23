using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using System.Net.Http.Headers;
using BlazingQuiz.Shared.Components.Auth;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazingQuiz.Shared.Components.Services
{
    public class CategoryImageService
    {
        private readonly HttpClient _httpClient;
        private readonly QuizAuthStateProvider _authStateProvider;

        public CategoryImageService(HttpClient httpClient, QuizAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<QuizApiResponse> UploadCategoryImageAsync(int categoryId, IBrowserFile file)
        {
            try
            {
                // Create multipart form data
                using var formData = new MultipartFormDataContent();
                
                // Create file content
                var fileContent = new StreamContent(file.OpenReadStream(10 * 1024 * 1024)); // 10MB limit
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "image", // Parameter name expected by the API
                    FileName = file.Name
                };
                
                formData.Add(fileContent);

                // Set the authorization header
                var token = _authStateProvider.User?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync($"/api/category-images/upload/{categoryId}", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return QuizApiResponse.Success();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return QuizApiResponse.Failure($"Failed to upload image. Status: {response.StatusCode}. Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure($"Exception: {ex.Message}");
            }
        }

        public async Task<QuizApiResponse> RemoveCategoryImageAsync(int categoryId)
        {
            try
            {
                var token = _authStateProvider.User?.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.DeleteAsync($"/api/category-images/remove/{categoryId}");

                if (response.IsSuccessStatusCode)
                {
                    return QuizApiResponse.Success();
                }
                else
                {
                    return QuizApiResponse.Failure("Failed to remove image");
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
            // The API base address is configured in Program.cs as "https://localhost:7048"
            return $"{_httpClient.BaseAddress}{imagePath}";
        }
    }
}