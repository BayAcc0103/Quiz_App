using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class QuizFeedback
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        public Guid QuizId { get; set; }
        public string? Score { get; set; } // Rating text (e.g., "bad", "very bad", "normal", "good", "very good")
        public string? Comment { get; set; } // Comment content
        
        [ForeignKey(nameof(StudentId))]
        public virtual User Student { get; set; }
        
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}