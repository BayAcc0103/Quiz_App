using System.Text.Json.Serialization;

namespace BlazingQuiz.Shared.DTOs
{
    public class RecommendedQuizDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string QuizId { get; set; } = string.Empty;
        public decimal PredictedRating { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public QuizDetailsDto? Quiz { get; set; } // This will be populated separately if needed
    }
}