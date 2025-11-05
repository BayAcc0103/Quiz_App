using System;

namespace BlazingQuiz.Shared.DTOs
{
    public record StudentQuizQuestionResponseDto(int StudentQuizId, int QuestionId, int OptionId, string? TextAnswer = null);
}