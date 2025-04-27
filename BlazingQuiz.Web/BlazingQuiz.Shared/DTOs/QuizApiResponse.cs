using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public record QuizApiResponse(bool IsSuccess, string? ErrorMessage)
    {
        public static QuizApiResponse Success() => new(true, null);
        public static QuizApiResponse Failure(string errorMessage) => new(false, errorMessage);
    }
}
