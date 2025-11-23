using System;

namespace BlazingQuiz.Shared.DTOs
{
    public class RoomHistoryDto
    {
        public Guid RoomId { get; set; }
        
        public string RoomName { get; set; } = string.Empty;
        
        public string RoomCode { get; set; } = string.Empty;
        
        public Guid QuizId { get; set; }
        
        public string QuizName { get; set; } = string.Empty;
        
        public int Rank { get; set; }
        
        public int CorrectAnswers { get; set; }
        
        public int TotalQuestions { get; set; }
        
        public TimeSpan? CompletionTime { get; set; }
        
        public DateTime StartedOn { get; set; }
        
        public DateTime? CompletedOn { get; set; }
        
        public string Status { get; set; } = string.Empty;
    }
}