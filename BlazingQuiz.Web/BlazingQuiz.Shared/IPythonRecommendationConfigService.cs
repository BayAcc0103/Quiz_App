using System;

namespace BlazingQuiz.Shared
{
    public interface IPythonRecommendationConfigService
    {
        string GetBaseUrl();
        void SetBaseUrl(string baseUrl);
    }
}