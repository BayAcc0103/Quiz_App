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
    public record QuizApiResponse<TData>(TData Data, bool IsSuccess, string? ErrorMessage)
    {
        public static QuizApiResponse<TData> Success(TData data) => new(data, true, null);
        public static QuizApiResponse<TData> Failure(string errorMessage) => new(default, false, errorMessage);
    }
}
