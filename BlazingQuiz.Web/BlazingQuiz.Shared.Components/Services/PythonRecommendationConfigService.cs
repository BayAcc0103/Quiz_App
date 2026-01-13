using System;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Shared.Components.Services
{
    public class PythonRecommendationConfigService : IPythonRecommendationConfigService
    {
        private string _baseUrl = "https://zf1jp10g-5000.asse.devtunnels.ms"; // Default value

        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                _baseUrl = baseUrl;
            }
        }
    }
}