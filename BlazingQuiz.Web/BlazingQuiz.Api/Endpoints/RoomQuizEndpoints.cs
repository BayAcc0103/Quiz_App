using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class RoomQuizEndpoints
    {
        public static int GetStudentIdForRoom(this ClaimsPrincipal principal) =>
           Convert.ToInt32(principal.FindFirstValue(ClaimTypes.NameIdentifier));
        public static IEndpointRouteBuilder MapRoomQuizEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/room-quiz")
                .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Student)));

            var quizGroup = group.MapGroup("/quiz");
            
            // Start room quiz - creates a StudentQuizForRoom record for the student
            quizGroup.MapPost("/{roomId:guid}/start", async (Guid roomId, ClaimsPrincipal principal, RoomQuizService quizService, QuizContext context) =>
            {
                var studentId = principal.GetStudentIdForRoom();
                
                // Verify that the student is part of the room
                var roomParticipants = await context.RoomParticipants
                    .Where(rp => rp.RoomId == roomId && rp.UserId == studentId)
                    .AnyAsync();
                    
                if (!roomParticipants)
                {
                    return Results.BadRequest("Student is not a participant in this room.");
                }
                
                // Get the quiz ID from the room
                var room = await context.Rooms.FindAsync(roomId);
                if (room?.QuizId == null)
                {
                    return Results.BadRequest("Room does not have a quiz assigned.");
                }
                
                var result = await quizService.StartQuizAsync(studentId, room.QuizId.Value, roomId);
                return Results.Ok(result);
            });

            // Get next question for room quiz
            quizGroup.MapGet("/{studentQuizForRoomId:int}/next-question", async (int studentQuizForRoomId, ClaimsPrincipal principal, RoomQuizService quizService) =>
                Results.Ok(await quizService.GetNextQuestionForQuizAsync(studentQuizForRoomId, principal.GetStudentIdForRoom())));

            // Save response for room quiz
            quizGroup.MapPost("/{studentQuizForRoomId:int}/save-response", async (int studentQuizForRoomId, StudentQuizQuestionResponseDto dto, ClaimsPrincipal principal, RoomQuizService quizService) =>
            {
                if(studentQuizForRoomId != dto.StudentQuizId)
                    return Results.Unauthorized();
                return Results.Ok(await quizService.SaveQuestionResponseAsync(dto, principal.GetStudentIdForRoom()));
            });

            // Submit room quiz
            quizGroup.MapPost("/{studentQuizForRoomId:int}/submit", async (int studentQuizForRoomId, ClaimsPrincipal principal, RoomQuizService quizService) =>
                Results.Ok(await quizService.SubmitQuizAsync(studentQuizForRoomId, principal.GetStudentIdForRoom())));

            // Auto-submit room quiz
            quizGroup.MapPost("/{studentQuizForRoomId:int}/auto-submit", async (int studentQuizForRoomId, ClaimsPrincipal principal, RoomQuizService quizService) =>
                Results.Ok(await quizService.AutoSubmitQuizAsync(studentQuizForRoomId, principal.GetStudentIdForRoom())));

            // Exit room quiz
            quizGroup.MapPost("/{studentQuizForRoomId:int}/exit", async (int studentQuizForRoomId, ClaimsPrincipal principal, RoomQuizService quizService) =>
                Results.Ok(await quizService.ExitQuizAsync(studentQuizForRoomId, principal.GetStudentIdForRoom())));

            // Get room quiz result
            quizGroup.MapGet("/{studentQuizForRoomId:int}/result", async (int studentQuizForRoomId, ClaimsPrincipal principal, RoomQuizService quizService) =>
                Results.Ok(await quizService.GetQuizResultAsync(studentQuizForRoomId, principal.GetStudentIdForRoom())));

            return app;
        }
    }
}