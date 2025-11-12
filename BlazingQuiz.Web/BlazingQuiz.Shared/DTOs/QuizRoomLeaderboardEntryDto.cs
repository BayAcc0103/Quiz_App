using System;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuizRoomLeaderboardEntryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentAvatarPath { get; set; }
        public int Total { get; set; }
        public TimeSpan? CompletionTime { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int Rank { get; set; }
    }
}