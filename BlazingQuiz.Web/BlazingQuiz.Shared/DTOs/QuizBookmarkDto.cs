using System;
using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuizBookmarkDto
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public Guid QuizId { get; set; }
        
        [MaxLength(100)]
        public string QuizName { get; set; }
        
        [MaxLength(100)]
        public string CategoryName { get; set; }
        
        public DateTime BookmarkedOn { get; set; }
    }
}