using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuizListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; } // Description of the quiz
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeInMinutes { get; set; }
        public bool IsActive { get; set; }
        public string? Level { get; set; } // Level of the quiz (e.g., Easy, Medium, Hard)
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedByAvatarPath { get; set; }
        public int? CreatedBy { get; set; } // User ID of the creator
        public DateTime? CreatedAt { get; set; }

        // Average rating from QuizFeedback
        public double? AverageRating { get; set; }

        // Collection of all categories for the quiz
        public List<string> AllCategoryNames { get; set; } = new List<string>();
    }
}
