using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        [Required, MaxLength(500)]
        public string Text { get; set; }
        public string? AnswerExplanation { get; set; } // Explanation of the correct answer
        public string? ImagePath { get; set; } // Path to the question image
        public string? AudioPath { get; set; } // Path to the question audio
        public List<OptionDto> Options { get; set; } = [];
        public List<TextAnswerDto> TextAnswers { get; set; } = [];
        public bool IsTextAnswer { get; set; } = false;
        public DateTime? CreatedAt { get; set; } // Timestamp when the question was created
        public int? CreatedBy { get; set; } // User ID of who created the question
        public int Points { get; set; } = 1; // Points for the question
    }
}
