using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class StudentQuizEndpoints
    {
        public static int GetStudentId(this ClaimsPrincipal principal) => 
            Convert.ToInt32(principal.FindFirstValue(ClaimTypes.NameIdentifier));
        public static IEndpointRouteBuilder MapStudentQuizEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/student")
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Student)));

            group.MapGet("/available-quizes", async (int categoryId, StudentQuizService quizService) =>
                Results.Ok(await quizService.GetActiveQuizesAsync(categoryId)));

            group.MapGet("/my-quizes", async (int startIndex, int pageSize, StudentQuizService quizService, ClaimsPrincipal principal) =>
                Results.Ok(await quizService.GetStudentQuizesAsync(principal.GetStudentId(), startIndex, pageSize)));

            var quizGroup = group.MapGroup("/quiz");
            quizGroup.MapPost("/{quizId:guid}/start", async (Guid quizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.StartQuizAsync(principal.GetStudentId(), quizId)));

            quizGroup.MapGet("/{studentQuizId:int}/next-question", async (int studentQuizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.GetNextQuestionForQuizAsync(studentQuizId, principal.GetStudentId())));

            quizGroup.MapPost("/{studentQuizId:int}/save-response", async (int studentQuizId, StudentQuizQuestionResponseDto dto, ClaimsPrincipal principal, StudentQuizService quizService) =>
            {
                if(studentQuizId != dto.StudentQuizId)
                    return Results.Unauthorized();
                return Results.Ok(await quizService.SaveQuestionResponseAsync(dto, principal.GetStudentId()));
            });

            quizGroup.MapPost("/{studentQuizId:int}/submit", async (int studentQuizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.SubmitQuizAsync(studentQuizId, principal.GetStudentId())));

            quizGroup.MapPost("/{studentQuizId:int}/auto-submit", async (int studentQuizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.AutoSubmitQuizAsync(studentQuizId, principal.GetStudentId())));

            quizGroup.MapPost("/{studentQuizId:int}/exit", async (int studentQuizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.ExitQuizAsync(studentQuizId, principal.GetStudentId())));

            quizGroup.MapGet("/{studentQuizId:int}/result", async (int studentQuizId, ClaimsPrincipal principal, StudentQuizService quizService) =>
                Results.Ok(await quizService.GetQuizResultAsync(studentQuizId, principal.GetStudentId())));
            return app;
        }
    }
}
