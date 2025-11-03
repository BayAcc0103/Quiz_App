using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
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

            return app;
        }
    }
}