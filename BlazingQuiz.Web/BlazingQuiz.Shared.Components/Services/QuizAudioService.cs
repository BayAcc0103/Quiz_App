using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using BlazingQuiz.Shared.Components.Auth;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace BlazingQuiz.Shared.Components.Services;

public class QuizAudioService
{
    private readonly HttpClient _httpClient;
    private readonly QuizAuthStateProvider _authStateProvider;
    private const string ApiServer = "https://b861mvjb-7048.asse.devtunnels.ms"; // Match the API server address

    public QuizAudioService(HttpClient httpClient, QuizAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<QuizApiResponse> UploadQuizAudioAsync(Guid quizId, IBrowserFile audioFile)
    {
        try
        {
            // Check if user is authenticated and has the required role
            var user = _authStateProvider.User;
            if (user == null ||
                !(user.Role == UserRole.Admin.ToString() || user.Role == UserRole.Teacher.ToString()))
            {
                return QuizApiResponse.Failure("Only admins and teachers can upload quiz audio");
            }

            using var formData = new MultipartFormDataContent();

            // Create file content with proper headers
            var fileContent = new StreamContent(audioFile.OpenReadStream(20 * 1024 * 1024)); // 20MB limit
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(audioFile.ContentType);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "audio", // Parameter name expected by the API
                FileName = audioFile.Name
            };

            formData.Add(fileContent);

            // Set the authorization header
            var token = _authStateProvider.User?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsync($"/api/quiz-audios/upload/{quizId}", formData);

            if (response.IsSuccessStatusCode)
            {
                return QuizApiResponse.Success();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return QuizApiResponse.Failure($"Failed to upload audio. Status: {response.StatusCode}. Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return QuizApiResponse.Failure($"Exception: {ex.Message}");
        }
    }

    public async Task<QuizApiResponse> RemoveQuizAudioAsync(Guid quizId)
    {
        try
        {
            // Check if user is authenticated and has the required role
            var user = _authStateProvider.User;
            if (user == null ||
                !(user.Role == UserRole.Admin.ToString() || user.Role == UserRole.Teacher.ToString()))
            {
                return QuizApiResponse.Failure("Only admins and teachers can remove quiz audio");
            }

            // Set the authorization header
            var token = _authStateProvider.User?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.DeleteAsync($"/api/quiz-audios/remove/{quizId}");

            if (response.IsSuccessStatusCode)
            {
                return QuizApiResponse.Success();
            }
            else
            {
                return QuizApiResponse.Failure("Failed to remove audio");
            }
        }
        catch (Exception ex)
        {
            return QuizApiResponse.Failure(ex.Message);
        }
    }

    public string GetAudioUrl(string audioPath)
    {
        if (string.IsNullOrEmpty(audioPath))
            return string.Empty;

        // Construct the full URL to the audio on the API server
        return $"{ApiServer}/{audioPath}";
    }
}