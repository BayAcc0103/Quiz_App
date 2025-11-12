using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazingQuiz.Api.Endpoints
{
    public static class RoomEndpoints
    {
        public static IEndpointRouteBuilder MapRoomEndpoints(this IEndpointRouteBuilder app)
        {
            var roomGroup = app.MapGroup("/api/rooms").RequireAuthorization();
            
            // Create room - teachers, admins, and students can do this
            roomGroup.MapPost("", async (CreateRoomDto dto, RoomService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var room = await service.CreateRoomAsync(dto.Name, dto.Description, userId, dto.MaxParticipants, dto.QuizId);
                
                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Code = room.Code,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByName = room.CreatedByUser?.Name,
                    QuizId = room.QuizId,
                    CreatedAt = room.CreatedAt,
                    IsActive = room.IsActive,
                    MaxParticipants = room.MaxParticipants
                };

                return Results.Ok(roomDto);
            }).RequireAuthorization(); // Allow any authenticated user (admin, teacher, student)

            // Get room by code
            roomGroup.MapGet("/code/{code}", async (string code, RoomService service) =>
            {
                var room = await service.GetRoomByCodeAsync(code);
                if (room == null)
                {
                    return Results.NotFound("Room not found.");
                }

                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Code = room.Code,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByName = room.CreatedByUser?.Name,
                    QuizId = room.QuizId,
                    QuizName = room.Quiz?.Name, // Include quiz name
                    CreatedAt = room.CreatedAt,
                    StartedAt = room.StartedAt,
                    EndedAt = room.EndedAt,
                    IsActive = room.IsActive,
                    MaxParticipants = room.MaxParticipants
                };

                return Results.Ok(roomDto);
            });

            // Get room by ID
            roomGroup.MapGet("/{id:guid}", async (Guid id, RoomService service) =>
            {
                var room = await service.GetRoomByIdAsync(id);
                if (room == null)
                {
                    return Results.NotFound("Room not found.");
                }

                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Code = room.Code,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByName = room.CreatedByUser?.Name,
                    QuizId = room.QuizId,
                    QuizName = room.Quiz?.Name, // Include quiz name
                    CreatedAt = room.CreatedAt,
                    StartedAt = room.StartedAt,
                    EndedAt = room.EndedAt,
                    IsActive = room.IsActive,
                    MaxParticipants = room.MaxParticipants
                };

                return Results.Ok(roomDto);
            });

            // Get rooms created by current user
            roomGroup.MapGet("", async (RoomService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var rooms = await service.GetRoomsByCreatorAsync(userId);
                var roomDtos = rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedBy = r.CreatedBy,
                    CreatedByName = r.CreatedByUser?.Name,
                    QuizId = r.QuizId,
                    QuizName = r.Quiz?.Name, // Include quiz name
                    CreatedAt = r.CreatedAt,
                    StartedAt = r.StartedAt,
                    EndedAt = r.EndedAt,
                    IsActive = r.IsActive,
                    MaxParticipants = r.MaxParticipants
                }).ToList();

                return Results.Ok(roomDtos);
            }).RequireAuthorization(); // Allow any authenticated user (admin, teacher, student)

            // Join room
            roomGroup.MapPost("/join", async (JoinRoomDto dto, RoomService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var success = await service.JoinRoomAsync(dto.Code, userId);
                if (!success)
                {
                    // Check if room exists but is full
                    var roomCheck = await service.GetRoomByCodeAsync(dto.Code);
                    if (roomCheck != null)
                    {
                        // Room exists but user couldn't join, check if it's full by getting participant count
                        var participants = await service.GetRoomParticipantsAsync(roomCheck.Id);
                        if (participants.Count >= roomCheck.MaxParticipants)
                        {
                            return Results.Conflict("Room is full. Cannot join room that has reached maximum capacity.");
                        }
                    }
                    return Results.NotFound("Room not found or could not join room.");
                }

                var room = await service.GetRoomByCodeAsync(dto.Code);
                if (room == null)
                {
                    return Results.NotFound("Room not found.");
                }

                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Code = room.Code,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByName = room.CreatedByUser?.Name,
                    CreatedAt = room.CreatedAt,
                    StartedAt = room.StartedAt,
                    EndedAt = room.EndedAt,
                    IsActive = room.IsActive,
                    MaxParticipants = room.MaxParticipants
                };

                return Results.Ok(roomDto);
            });

            // Get room participants
            roomGroup.MapGet("/{roomId:guid}/participants", async (Guid roomId, RoomService service) =>
            {
                var roomParticipants = await service.GetRoomParticipantsWithReadyStatusAsync(roomId);
                var participantDtos = roomParticipants.Select(rp => new RoomParticipantDto
                {
                    UserId = rp.User.Id,
                    UserName = rp.User.Name,
                    AvatarPath = rp.User.AvatarPath,
                    IsReady = rp.IsReady
                }).ToArray();

                return Results.Ok(participantDtos);
            });

            // Set participant ready status
            roomGroup.MapPost("/{roomId:guid}/ready", async (Guid roomId, HttpContext httpContext, RoomService service) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var success = await service.SetParticipantReadyStatusAsync(roomId, userId, true);
                if (!success)
                {
                    return Results.NotFound("Room or participant not found.");
                }

                return Results.Ok(new { Message = "Ready status set successfully" });
            });

            // Set participant not ready status
            roomGroup.MapPost("/{roomId:guid}/not-ready", async (Guid roomId, HttpContext httpContext, RoomService service) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var success = await service.SetParticipantReadyStatusAsync(roomId, userId, false);
                if (!success)
                {
                    return Results.NotFound("Room or participant not found.");
                }

                return Results.Ok(new { Message = "Not ready status set successfully" });
            });

            // Start room (for host only)
            roomGroup.MapPost("/{roomId:guid}/start", async (Guid roomId, HttpContext httpContext, RoomService service) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var result = await service.StartRoomAsync(roomId, userId);
                if (result == "Room not found")
                {
                    return Results.NotFound("Room not found.");
                }
                else if (result == "Not authorized")
                {
                    return Results.StatusCode(403); // Forbidden status code
                }
                else if (result == "Not enough participants")
                {
                    return Results.BadRequest("Not enough participants to start the room.");
                }
                else if (result == "Not all participants ready")
                {
                    return Results.BadRequest("All participants must be ready to start the room.");
                }
                else if (result == "Success")
                {
                    return Results.Ok(new { Message = "Room started successfully." });
                }
                else
                {
                    return Results.StatusCode(500); // Internal server error
                }
            });

            // Start quiz in room (for host only)
            roomGroup.MapPost("/{roomId:guid}/start-quiz", async (Guid roomId, HttpContext httpContext, RoomService service, QuizContext context) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Check if user is the host
                var room = await service.GetRoomByIdAsync(roomId);
                if (room == null)
                {
                    return Results.NotFound("Room not found.");
                }

                if (room.CreatedBy != userId)
                {
                    return Results.StatusCode(403); // Forbidden
                }

                if (!room.IsActive)
                {
                    return Results.BadRequest("Room is not active.");
                }

                if (room.StartedAt == null)
                {
                    return Results.BadRequest("Room has not been started yet.");
                }

                // Update room status to indicate quiz has started
                await service.UpdateRoomStatusAsync(roomId, startedAt: room.StartedAt);

                // Send real-time update to all participants in the room that the quiz has started
                await service.NotifyQuizStartedAsync(room.Code);

                return Results.Ok(new { Message = "Quiz started successfully.", RoomCode = room.Code });
            });

            // Submit answer for a room quiz
            roomGroup.MapPost("/{roomId:guid}/answers", async (RoomAnswerDto dto, Guid roomId, HttpContext httpContext, RoomService service, QuizContext context) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Verify that the user is a participant in the room
                var isParticipant = await service.IsUserRoomParticipantAsync(roomId, userId);
                if (!isParticipant)
                {
                    return Results.BadRequest("User is not a participant in this room.");
                }

                // Verify that the question exists
                var question = await context.Questions
                    .FirstOrDefaultAsync(q => q.Id == dto.QuestionId);
                if (question == null)
                {
                    return Results.BadRequest("Question does not exist.");
                }

                // Verify that if it's a multiple choice question, the selected option is valid
                if (dto.OptionId.HasValue)
                {
                    var option = await context.Options
                        .FirstOrDefaultAsync(o => o.Id == dto.OptionId && o.QuestionId == dto.QuestionId);
                    if (option == null)
                    {
                        return Results.BadRequest("Selected option is not valid for this question.");
                    }
                }

                // Verify that the room is still active (not ended)
                var room = await context.Rooms
                    .FirstOrDefaultAsync(r => r.Id == roomId);
                if (room == null)
                {
                    return Results.BadRequest("Room does not exist.");
                }
                
                if (room.EndedAt.HasValue)
                {
                    return Results.BadRequest("Quiz in this room has already ended.");
                }

                // Record the answer
                var success = await service.RecordRoomAnswerAsync(roomId, userId, dto.QuestionId, dto.OptionId, dto.TextAnswer);
                if (!success)
                {
                    return Results.StatusCode(500);
                }

                return Results.Ok(new { Message = "Answer recorded successfully." });
            });

            // Get answers for a room
            roomGroup.MapGet("/{roomId:guid}/answers", async (Guid roomId, RoomService service) =>
            {
                var answers = await service.GetRoomAnswersAsync(roomId);
                var answerDtos = answers.Select(ra => new RoomAnswerDto
                {
                    Id = ra.Id,
                    RoomId = ra.RoomId,
                    UserId = ra.UserId,
                    QuestionId = ra.QuestionId,
                    OptionId = ra.OptionId,
                    TextAnswer = ra.TextAnswer,
                    AnsweredAt = ra.AnsweredAt
                }).ToList();

                return Results.Ok(answerDtos);
            });

            // Get answers for a specific user in a room
            roomGroup.MapGet("/{roomId:guid}/answers/user/{userId:int}", async (Guid roomId, int userId, RoomService service) =>
            {
                var answers = await service.GetRoomAnswersForUserAsync(roomId, userId);
                var answerDtos = answers.Select(ra => new RoomAnswerDto
                {
                    Id = ra.Id,
                    RoomId = ra.RoomId,
                    UserId = ra.UserId,
                    QuestionId = ra.QuestionId,
                    OptionId = ra.OptionId,
                    TextAnswer = ra.TextAnswer,
                    AnsweredAt = ra.AnsweredAt
                }).ToList();

                return Results.Ok(answerDtos);
            });

            // Submit quiz to room - marks participant as having submitted
            roomGroup.MapPost("/{roomId:guid}/submit", async (Guid roomId, HttpContext httpContext, RoomService service) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Try to read studentQuizId from the request body
                var request = httpContext.Request;
                int studentQuizId = 0;
                
                // Check if there's a body and try to read it
                if (request.ContentLength > 0)
                {
                    using var reader = new StreamReader(request.Body);
                    var bodyContent = await reader.ReadToEndAsync();
                    
                    // Try to parse the studentQuizId from JSON
                    try
                    {
                        var jsonDocument = System.Text.Json.JsonDocument.Parse(bodyContent);
                        if (jsonDocument.RootElement.TryGetProperty("studentQuizId", out var idElement))
                        {
                            studentQuizId = idElement.GetInt32();
                        }
                        else if (jsonDocument.RootElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            // If the body is just the number, parse it directly
                            studentQuizId = jsonDocument.RootElement.GetInt32();
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // If JSON parsing fails, try to parse as plain number
                        if (int.TryParse(bodyContent.Trim(), out var parsedId))
                        {
                            studentQuizId = parsedId;
                        }
                    }
                }

                // Verify that the user is a participant in the room
                var participants = await service.GetRoomParticipantsAsync(roomId);
                if (!participants.Any(p => p.Id == userId))
                {
                    return Results.BadRequest("User is not a participant in this room.");
                }

                // Mark the participant as having submitted their quiz
                var success = await service.SetParticipantSubmissionStatusAsync(roomId, userId, true);
                if (!success)
                {
                    return Results.StatusCode(500);
                }

                // Check if all participants have submitted
                var allParticipants = await service.GetRoomParticipantsWithSubmissionStatusAsync(roomId);
                var totalParticipants = allParticipants.Count;
                var submittedParticipants = allParticipants.Count(p => p.HasSubmitted);

                if (totalParticipants > 0 && submittedParticipants == totalParticipants)
                {
                    // All participants have submitted, notify all via SignalR
                    await service.NotifyQuizEndedAsync((await service.GetRoomByIdAsync(roomId)).Code);
                }

                return Results.Ok(new { Message = "Quiz submission recorded successfully." });
            });

            // Get submission status for all participants in a room
            roomGroup.MapGet("/{roomId:guid}/submission-status", async (Guid roomId, RoomService service) =>
            {
                var participants = await service.GetRoomParticipantsWithSubmissionStatusAsync(roomId);
                var participantDtos = participants.Select(rp => new RoomParticipantDto
                {
                    UserId = rp.User.Id,
                    UserName = rp.User.Name,
                    AvatarPath = rp.User.AvatarPath,
                    IsReady = rp.IsReady,
                    HasSubmitted = rp.HasSubmitted
                }).ToArray();

                return Results.Ok(participantDtos);
            });

            // Get all rooms for admin
            roomGroup.MapGet("/admin", async (RoomService service) =>
            {
                var rooms = await service.GetRoomsForAdminAsync();
                var roomDtos = rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedBy = r.CreatedBy,
                    CreatedByName = r.CreatedByUser?.Name,
                    QuizId = r.QuizId,
                    QuizName = r.Quiz?.Name,
                    CreatedAt = r.CreatedAt,
                    StartedAt = r.StartedAt,
                    EndedAt = r.EndedAt,
                    IsActive = r.IsActive,
                    MaxParticipants = r.MaxParticipants
                }).ToList();

                return Results.Ok(roomDtos);
            }).RequireAuthorization(policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));

            // Delete room - admin only
            roomGroup.MapDelete("/{roomId:guid}", async (Guid roomId, RoomService service) =>
            {
                var success = await service.DeleteRoomAsync(roomId);
                if (!success)
                {
                    return Results.NotFound("Room not found.");
                }

                return Results.Ok(new { Message = "Room and all associated participants and answers deleted successfully." });
            }).RequireAuthorization(policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));

            // Remove participant from room - host only
            roomGroup.MapDelete("/{roomId:guid}/participants/{userId:int}", async (Guid roomId, int userId, HttpContext httpContext, RoomService service) =>
            {
                // Get the current user ID from the claims
                var currentUserIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(currentUserIdString, out var currentUserId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                var success = await service.RemoveParticipantFromRoomAsync(roomId, userId, currentUserId);
                if (!success)
                {
                    return Results.NotFound("Room not found, you are not the host, or participant not found in the room.");
                }

                return Results.Ok(new { Message = "Participant removed from room successfully." });
            });

            // Get leaderboard for a room
            roomGroup.MapGet("/{roomId:guid}/leaderboard", async (Guid roomId, QuizContext context) =>
            {
                var studentQuizzes = await context.StudentQuizzesForRoom
                    .Where(sqfr => sqfr.RoomId == roomId && sqfr.Status == "Completed")
                    .Include(sqfr => sqfr.Student)
                    .ToListAsync();

                if (!studentQuizzes.Any())
                {
                    return Results.Ok(new List<QuizRoomLeaderboardEntryDto>());
                }

                // Calculate completion time for each student
                var leaderboardEntries = studentQuizzes.Select(sqfr =>
                {
                    TimeSpan? completionTime = null;
                    if (sqfr.StartedOn != DateTime.MinValue && sqfr.CompletedOn.HasValue)
                    {
                        completionTime = sqfr.CompletedOn.Value - sqfr.StartedOn;
                    }

                    return new QuizRoomLeaderboardEntryDto
                    {
                        StudentId = sqfr.StudentId,
                        StudentName = sqfr.Student?.Name ?? "Unknown",
                        StudentAvatarPath = sqfr.Student?.AvatarPath,
                        Total = sqfr.Total,
                        CompletionTime = completionTime,
                        StartedOn = sqfr.StartedOn,
                        CompletedOn = sqfr.CompletedOn
                    };
                }).ToList();

                // Sort by Total (descending) then by CompletionTime (ascending) if available
                var sortedLeaderboard = leaderboardEntries
                    .OrderByDescending(entry => entry.Total)
                    .ThenBy(entry => entry.CompletionTime ?? TimeSpan.MaxValue)
                    .ToList();

                // Assign ranks
                for (int i = 0; i < sortedLeaderboard.Count; i++)
                {
                    sortedLeaderboard[i].Rank = i + 1;
                }

                return Results.Ok(sortedLeaderboard);
            });

            return app;
        }
    }
}