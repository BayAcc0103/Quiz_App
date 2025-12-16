using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class RecommendedQuiz
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public string QuizId { get; set; } = string.Empty;  // Using string to handle GUID
        public decimal PredictedRating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        // Navigation property would require mapping to Quiz entity differently
        // For now, we'll keep QuizId as string
    }
}