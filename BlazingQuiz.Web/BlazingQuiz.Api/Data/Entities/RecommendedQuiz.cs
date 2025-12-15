using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class RecommendedQuiz
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string QuizId { get; set; }  // Using string since it's a GUID in the database

        [Column(TypeName = "decimal(18,2)")]
        public decimal PredictedRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        // Note: QuizId is stored as string but references the Quiz.Id which is a GUID
    }
}