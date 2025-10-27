using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        public Guid QuizId { get; set; }
        public int Score { get; set; } // Rating as integer (1-5 scale)
        
        [ForeignKey(nameof(StudentId))]
        public virtual User Student { get; set; }
        
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}