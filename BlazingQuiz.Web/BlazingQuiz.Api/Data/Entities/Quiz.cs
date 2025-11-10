using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Quiz
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; } // Description of the quiz
        public int? CategoryId { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeInMinutes { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; } // Teacher ID who created the quiz (nullable for existing quizzes)
        public string? ImagePath { get; set; } // Path to the quiz image
        public string? AudioPath { get; set; } // Path to the quiz audio
        public string? Level { get; set; } // Level of the quiz (e.g., Easy, Medium, Hard)
        
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }
        
        public virtual ICollection<Question> Questions { get; set; } = [];
        public virtual ICollection<QuizFeedback> QuizFeedbacks { get; set; } = [];
        public virtual ICollection<Rating> Ratings { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];
        public virtual ICollection<QuizCategory> QuizCategories { get; set; } = [];
    }
}
