using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class QuizEndpoints
    {
        public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder app)
        {
            var quizGroup = app.MapGroup("/api/quizes").RequireAuthorization();
            quizGroup.MapPost("", async (QuizSaveDto dto, QuizService service, HttpContext httpContext) =>
            {
                if (dto.Questions.Count == 0)
                {
                    return Results.BadRequest("Please provide Questions ");
                }
                if (dto.Questions.Count != dto.TotalQuestions)
                {
                    return Results.BadRequest("Total Questions count does not match with provided questions");
                }

                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                return Results.Ok(await service.SaveQuizAsync(dto, userId));
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            quizGroup.MapGet("", async (QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Check user role to determine if they should see all quizzes or just their own
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                var quizes = await service.GetQuizesAsync(userId, userRole == nameof(UserRole.Admin));
                return Results.Ok(quizes);
            });
            quizGroup.MapGet("{quizId:guid}/questions", async (Guid quizId, QuizService service) =>
            {
                return Results.Ok(await service.GetQuizQuestionsAsync(quizId));
            });
            quizGroup.MapGet("{quizId:guid}", async (Guid quizId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Check user role to determine if they should see all quizzes or just their own
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                var quiz = await service.GetQuizToEditAsync(quizId, userId, userRole == nameof(UserRole.Admin));
                if (quiz == null)
                {
                    return Results.NotFound("Quiz not found or you don't have permission to access it.");
                }
                
                return Results.Ok(quiz);
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            return app;
        }
    }
}
