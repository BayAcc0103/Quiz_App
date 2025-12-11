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
            
            quizGroup.MapGet("{quizId:guid}/feedback", async (Guid quizId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Check user role to determine if they have permission to view feedback
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Forbid();
                }

                var feedback = await service.GetQuizFeedbackAsync(quizId, userId, userRole == nameof(UserRole.Admin));
                if (feedback == null)
                {
                    return Results.NotFound("Quiz not found or you don't have permission to access feedback.");
                }
                
                return Results.Ok(feedback);
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            
            quizGroup.MapDelete("feedback/{feedbackId:int}", async (int feedbackId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete feedback
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this feedback."));
                }

                var result = await service.DeleteQuizFeedbackAsync(feedbackId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Feedback not found or you don't have permission to delete it."));
                }
                
                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            
            quizGroup.MapDelete("ratings/{ratingId:int}", async (int ratingId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete rating
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this rating."));
                }

                var result = await service.DeleteRatingAsync(ratingId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Rating not found or you don't have permission to delete it."));
                }
                
                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));
            
            quizGroup.MapDelete("comments/{commentId:int}", async (int commentId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete comment
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this comment."));
                }

                var result = await service.DeleteCommentAsync(commentId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Comment not found or you don't have permission to delete it."));
                }

                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            quizGroup.MapDelete("options/{optionId:int}", async (int optionId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete option
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this option."));
                }

                var result = await service.DeleteOptionAsync(optionId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Option not found or you don't have permission to delete it."));
                }

                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            quizGroup.MapDelete("questions/{questionId:int}", async (int questionId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete question
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this question."));
                }

                var result = await service.DeleteQuestionAsync(questionId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Question not found or you don't have permission to delete it."));
                }

                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            quizGroup.MapDelete("{quizId:guid}", async (Guid quizId, QuizService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.Ok(QuizApiResponse.Failure("Invalid user ID"));
                }

                // Check user role to determine if they have permission to delete quiz
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Admin) && userRole != nameof(UserRole.Teacher))
                {
                    return Results.Ok(QuizApiResponse.Failure("You don't have permission to delete this quiz."));
                }

                var result = await service.DeleteQuizAsync(quizId, userId, userRole == nameof(UserRole.Admin));
                if (!result)
                {
                    return Results.Ok(QuizApiResponse.Failure("Quiz not found or you don't have permission to delete it."));
                }

                return Results.Ok(QuizApiResponse.Success());
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));


            return app;
        }
    }
}
