using System;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Web.Services
{
    public class PythonRecommendationConfigService : IPythonRecommendationConfigService
    {
        private string _baseUrl = "http://localhost:5000"; // Default value

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