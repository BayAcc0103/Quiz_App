using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared.DTOs;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuizEndpoints
    {
        public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
        {
            var quizGroup = app.MapGroup("/api/quizes").RequireAuthorization();
            quizGroup.MapPost("", async (QuizSaveDto dto, QuizService service) =>
            {
                if (dto.Questions.Count == 0)
                {
                    return Results.BadRequest("Please provide Questions ");
                }
                if (dto.Questions.Count != dto.TotalQuestions)
                {
                    return Results.BadRequest("Total Questions count does not match with provided questions");
                }

                return Results.Ok(await service.SaveQuizAsync(dto));
            });

            return app;
        }
    }
}
