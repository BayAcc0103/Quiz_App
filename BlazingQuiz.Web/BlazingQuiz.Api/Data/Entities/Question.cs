using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(500)]
        public string Text { get; set; }
        public string? ImagePath { get; set; } // Path to the question image
        public Guid QuizId { get; set; }
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }

        public virtual ICollection<Option> Options { get; set; } = [];
        public virtual ICollection<StudentQuizQuestion> StudentQuizQuestions { get; set; } = [];
    }
}
