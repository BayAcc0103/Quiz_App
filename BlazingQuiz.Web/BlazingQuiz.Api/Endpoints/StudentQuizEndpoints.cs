using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Api.Endpoints
{
    public static class StudentQuizEndpoints
    {
        public static IEndpointRouteBuilder MapStudentQuizEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/student")
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Student)));

            group.MapGet("/available-quizes", async (int categoryId, StudentQuizService quizService) =>
                Results.Ok(await quizService.GetActiveQuizesAsync(categoryId))
            );

            return app;
        }
    }
}
