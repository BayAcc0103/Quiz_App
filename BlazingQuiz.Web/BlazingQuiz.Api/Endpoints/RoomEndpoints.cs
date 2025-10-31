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
            
            // Create room - only teachers and admins can do this
            roomGroup.MapPost("", async (CreateRoomDto dto, RoomService service, HttpContext httpContext) =>
            {
                // Get the current user ID from the claims
                var userIdString = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                {
                    return Results.BadRequest("Invalid user ID");
                }

                // Check user role to determine if they have permission to create a room
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != nameof(UserRole.Teacher) && userRole != nameof(UserRole.Admin))
                {
                    return Results.Forbid();
                }

                var room = await service.CreateRoomAsync(dto.Name, dto.Description, userId, dto.MaxParticipants);
                
                var roomDto = new RoomDto
                {
                    Id = room.Id,
                    Code = room.Code,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByName = room.CreatedByUser?.Name,
                    CreatedAt = room.CreatedAt,
                    IsActive = room.IsActive,
                    MaxParticipants = room.MaxParticipants
                };

                return Results.Ok(roomDto);
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

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
                    CreatedAt = r.CreatedAt,
                    StartedAt = r.StartedAt,
                    EndedAt = r.EndedAt,
                    IsActive = r.IsActive,
                    MaxParticipants = r.MaxParticipants
                }).ToList();

                return Results.Ok(roomDtos);
            }).RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

            return app;
        }
    }
}