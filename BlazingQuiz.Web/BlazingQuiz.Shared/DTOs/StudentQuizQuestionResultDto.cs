using System;

namespace BlazingQuiz.Shared.DTOs
{
    public class StudentQuizQuestionResultDto
    {
        public int Id { get; set; }
        public int StudentQuizId { get; set; }
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public string? TextAnswer { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}