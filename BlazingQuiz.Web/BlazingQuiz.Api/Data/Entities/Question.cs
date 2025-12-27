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
        public string? AnswerExplanation { get; set; } // Explanation of the correct answer
        public string? ImagePath { get; set; } // Path to the question image
        public string? AudioPath { get; set; } // Path to the question audio
        public bool IsTextAnswer { get; set; } = false; // True if it's a text input question
        public Guid? QuizId { get; set; }
        public DateTime? CreatedAt { get; set; } // Timestamp when the question was created
        public int? CreatedBy { get; set; } // User ID of who created the question

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }

        public virtual ICollection<Option> Options { get; set; } = [];
        public virtual ICollection<TextAnswer> TextAnswers { get; set; } = [];
        public virtual ICollection<StudentQuizQuestion> StudentQuizQuestions { get; set; } = [];
        public virtual ICollection<StudentQuizQuestionForRoom> StudentQuizQuestionsForRoom { get; set; } = [];
    }
}
