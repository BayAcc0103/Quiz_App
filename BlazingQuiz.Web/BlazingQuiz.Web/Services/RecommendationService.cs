using System.Text.Json;
using BlazingQuiz.Web.Auth;

namespace BlazingQuiz.Web.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly QuizAuthStateProvider _authStateProvider;

        public RecommendationService(HttpClient httpClient, QuizAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task TriggerRecommendationUpdateAsync()
        {
            if (_authStateProvider.IsLoggedIn && _authStateProvider.User?.Id > 0)
            {
                try
                {
                    var userId = _authStateProvider.User.Id;

                    // Call the API to trigger recommendation update
                    var response = await _httpClient.PostAsync($"/api/Recommendation/trigger/{userId}", null);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log error but don't fail the operation
                        Console.WriteLine($"Failed to trigger recommendation update: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error triggering recommendation update: {ex.Message}");
                }
            }
        }
    }
}