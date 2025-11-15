using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using BlazingQuiz.Shared.Components.Auth;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace BlazingQuiz.Shared.Components.Services;

public class QuizImageService
{
    private readonly HttpClient _httpClient;
    private readonly QuizAuthStateProvider _authStateProvider;
    private const string ApiServer = "https://b861mvjb-7048.asse.devtunnels.ms"; // Match the API server address

    public QuizImageService(HttpClient httpClient, QuizAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<QuizApiResponse> UploadQuizImageAsync(Guid quizId, IBrowserFile imageFile)
    {
        try
        {
            // Check if user is authenticated and has the required role
            var user = _authStateProvider.User;
            if (user == null ||
                !(user.Role == UserRole.Admin.ToString() || user.Role == UserRole.Teacher.ToString()))
            {
                return QuizApiResponse.Failure("Only admins and teachers can upload quiz images");
            }

            using var formData = new MultipartFormDataContent();

            // Create file content with proper headers
            var fileContent = new StreamContent(imageFile.OpenReadStream(10 * 1024 * 1024)); // 10MB limit
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "image", // Parameter name expected by the API
                FileName = imageFile.Name
            };

            formData.Add(fileContent);

            // Set the authorization header
            var token = _authStateProvider.User?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsync($"/api/quiz-images/upload/{quizId}", formData);

            if (response.IsSuccessStatusCode)
            {
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

    public async Task<QuizApiResponse> RemoveQuizImageAsync(Guid quizId)
    {
        try
        {
            // Check if user is authenticated and has the required role
            var user = _authStateProvider.User;
            if (user == null ||
                !(user.Role == UserRole.Admin.ToString() || user.Role == UserRole.Teacher.ToString()))
            {
                return QuizApiResponse.Failure("Only admins and teachers can remove quiz images");
            }

            // Set the authorization header
            var token = _authStateProvider.User?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.DeleteAsync($"/api/quiz-images/remove/{quizId}");

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
        return $"{ApiServer}/{imagePath}";
    }
}