using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuizSaveDto
    {
        public Guid Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; } // Description of the quiz
        public List<int> CategoryIds { get; set; } = new List<int>();
        [Range(1, int.MaxValue, ErrorMessage = "Please provide valid number of questions")]
        public int TotalQuestions { get; set; }
        [Range(1, 120, ErrorMessage = "Please provide valid time in minutes")]
        public int TimeInMinutes { get; set; }
        public bool IsActive { get; set; }
        public string? Level { get; set; } // Level of the quiz (e.g., Easy, Medium, Hard)
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }
        public List<QuestionDto> Questions { get; set; } = [];

        public string? Validate() 
        {
            if (CategoryIds == null || CategoryIds.Count == 0)
            {
                return "At least one category is required.";
            }
            
            if (TotalQuestions != Questions.Count)
            {
                return "Total Questions must be equal to the number of questions added.";
            }
            if (Questions.Any(q => string.IsNullOrWhiteSpace(q.Text)))
            {
                return "Question text is required.";
            }
            
            foreach (var q in Questions)
            {
                if (q.IsTextAnswer)
                {
                    // For text answer questions, ensure there's at least one correct text answer
                    if (q.TextAnswers == null || q.TextAnswers.Count == 0 || q.TextAnswers.All(ta => string.IsNullOrWhiteSpace(ta.Text)))
                    {
                        return $"At least one correct text answer is required for text input question: '{q.Text}'.";
                    }
                }
                else
                {
                    // For multiple choice questions, ensure there are at least 2 options
                    if (q.Options.Count < 2)
                    {
                        return "At-least 2 options are required for each multiple choice question.";
                    }
                    // For multiple choice questions, ensure there's a correct answer
                    if (!q.Options.Any(o => o.IsCorrect))
                    {
                        return "All multiple choice questions should have correct answer marked.";
                    }
                }
            }
            return null;
        }
    }
}
