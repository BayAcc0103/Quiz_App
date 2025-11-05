using System;

namespace BlazingQuiz.Shared.DTOs
{
    public class RoomParticipantLeaderboardResult
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarPath { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public TimeSpan? CompletionTime { get; set; }
    }
}