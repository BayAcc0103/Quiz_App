using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class RoomService
    {
        private readonly QuizContext _context;
        private readonly IHubContext<Hubs.QuizHub> _hubContext;

        public RoomService(QuizContext context, IHubContext<Hubs.QuizHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Room> CreateRoomAsync(string name, string? description, int createdBy, int maxParticipants = 50, Guid? quizId = null)
        {
            // Generate a unique 6-digit code
            string code = await GenerateUniqueRoomCodeAsync();
            
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Description = description,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                MaxParticipants = maxParticipants,
                QuizId = quizId,
                IsActive = true
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }

        private async Task<string> GenerateUniqueRoomCodeAsync()
        {
            string code;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 10; // Prevent infinite loop

            do
            {
                code = GenerateRandomCode(6);
                var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Code == code);
                isUnique = existingRoom == null;
                attempts++;
            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                throw new InvalidOperationException("Could not generate a unique room code after multiple attempts.");
            }

            return code;
        }

        private string GenerateRandomCode(int length)
        {
            var random = new Random();
            var code = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                code[i] = (char)('0' + random.Next(0, 10));
            }
            
            return new string(code);
        }

        public async Task<Room?> GetRoomByCodeAsync(string code)
        {
            return await _context.Rooms
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(r => r.Code == code && r.IsActive);
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id)
        {
            return await _context.Rooms
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<List<Room>> GetRoomsByCreatorAsync(int userId)
        {
            return await _context.Rooms
                .Include(r => r.CreatedByUser)
                .Include(r => r.Quiz) // Include quiz information
                .Where(r => r.CreatedBy == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> JoinRoomAsync(string code, int userId)
        {
            var room = await _context.Rooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Code == code && r.IsActive);

            if (room == null)
            {
                return false;
            }

            // Check if the room is full (reached maximum participants)
            if (room.Participants.Count >= room.MaxParticipants)
            {
                return false; // Room is full
            }

            // Check if user is already in the room
            var existingParticipant = room.Participants.FirstOrDefault(p => p.UserId == userId);
            if (existingParticipant != null)
            {
                return true; // Already joined
            }

            // Add user as participant
            var participant = new RoomParticipant
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.RoomParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<User>> GetRoomParticipantsAsync(Guid roomId)
        {
            return await _context.RoomParticipants
                .Where(rp => rp.RoomId == roomId)
                .Include(rp => rp.User)
                .Select(rp => rp.User)
                .ToListAsync();
        }

        public async Task<List<RoomParticipant>> GetRoomParticipantsWithReadyStatusAsync(Guid roomId)
        {
            return await _context.RoomParticipants
                .Where(rp => rp.RoomId == roomId)
                .Include(rp => rp.User)
                .ToListAsync();
        }

        public async Task<bool> IsUserRoomParticipantAsync(Guid roomId, int userId)
        {
            return await _context.RoomParticipants
                .AnyAsync(rp => rp.RoomId == roomId && rp.UserId == userId);
        }

        public async Task<bool> SetParticipantReadyStatusAsync(Guid roomId, int userId, bool isReady)
        {
            var roomParticipant = await _context.RoomParticipants
                .Include(rp => rp.User) // Need to include user to get the name
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);

            if (roomParticipant == null)
            {
                return false;
            }

            roomParticipant.IsReady = isReady;
            await _context.SaveChangesAsync();

            // Find the room to get the room code for SignalR group
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room != null)
            {
                // Send real-time update to all participants in the room
                await _hubContext.Clients.Group(room.Code)
                    .SendAsync("ParticipantReadyStatusChanged", roomParticipant.User.Name, isReady);
            }

            return true;
        }

        public async Task<string> StartRoomAsync(Guid roomId, int userId)
        {
            var room = await _context.Rooms
                .Include(r => r.Participants)
                    .ThenInclude(rp => rp.User)
                .FirstOrDefaultAsync(r => r.Id == roomId && r.IsActive);

            if (room == null)
            {
                return "Room not found";
            }

            // Check if the user is the host
            if (room.CreatedBy != userId)
            {
                return "Not authorized";
            }

            // Check if there are enough participants (at least 2)
            var participantCount = room.Participants.Count;
            if (participantCount < 2)
            {
                return "Not enough participants";
            }

            // Check if all participants are ready (excluding the host)
            var readyParticipantsCount = room.Participants.Count(rp => rp.IsReady);
            // All participants except the host need to be ready
            if (readyParticipantsCount < participantCount - 1)
            {
                return "Not all participants ready";
            }

            // Mark the host as ready when they click Start
            var hostParticipant = room.Participants.FirstOrDefault(rp => rp.UserId == userId);
            if (hostParticipant != null && !hostParticipant.IsReady)
            {
                hostParticipant.IsReady = true;
                
                // Send real-time update to all participants about the host's readiness status
                await _hubContext.Clients.Group(room.Code).SendAsync("ParticipantReadyStatusChanged", hostParticipant.User.Name, true);
            }

            // All conditions met, start the room
            room.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send real-time update to all participants in the room that the room has started
            await _hubContext.Clients.Group(room.Code).SendAsync("QuizStarted", room.Code);

            return "Success";
        }

        public async Task<bool> RecordRoomAnswerAsync(Guid roomId, int userId, int questionId, int? optionId, string? textAnswer)
        {
            // Check if user already answered this question in this room to prevent duplicates
            var existingAnswer = await _context.RoomAnswers
                .FirstOrDefaultAsync(ra => ra.RoomId == roomId && ra.UserId == userId && ra.QuestionId == questionId);
            
            RoomAnswer roomAnswer;
            if (existingAnswer != null)
            {
                // Update the existing answer instead of creating a new one
                existingAnswer.OptionId = optionId;
                existingAnswer.TextAnswer = textAnswer;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
                roomAnswer = existingAnswer;
            }
            else
            {
                // Create a new answer
                roomAnswer = new RoomAnswer
                {
                    RoomId = roomId,
                    UserId = userId,
                    QuestionId = questionId,
                    OptionId = optionId,
                    TextAnswer = textAnswer,
                    AnsweredAt = DateTime.UtcNow
                };
                
                _context.RoomAnswers.Add(roomAnswer);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RoomAnswer>> GetRoomAnswersAsync(Guid roomId)
        {
            return await _context.RoomAnswers
                .Where(ra => ra.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<List<RoomAnswer>> GetRoomAnswersForUserAsync(Guid roomId, int userId)
        {
            return await _context.RoomAnswers
                .Where(ra => ra.RoomId == roomId && ra.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UpdateRoomStatusAsync(Guid roomId, DateTime? startedAt = null, DateTime? endedAt = null)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return false;
            }

            if (startedAt.HasValue)
            {
                room.StartedAt = startedAt.Value;
            }

            if (endedAt.HasValue)
            {
                room.EndedAt = endedAt.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task NotifyQuizStartedAsync(string roomCode)
        {
            // Send real-time update to all participants in the room that the quiz has started
            await _hubContext.Clients.Group(roomCode).SendAsync("QuizStarted", roomCode);
        }

        public async Task<bool> SetParticipantSubmissionStatusAsync(Guid roomId, int userId, bool hasSubmitted)
        {
            var roomParticipant = await _context.RoomParticipants
                .Include(rp => rp.User) // Need to include user to get the name
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);

            if (roomParticipant == null)
            {
                return false;
            }

            roomParticipant.HasSubmitted = hasSubmitted;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RoomParticipant>> GetRoomParticipantsWithSubmissionStatusAsync(Guid roomId)
        {
            return await _context.RoomParticipants
                .Where(rp => rp.RoomId == roomId)
                .Include(rp => rp.User)
                .ToListAsync();
        }

        public async Task<List<Room>> GetRoomsForAdminAsync()
        {
            return await _context.Rooms
                .Include(r => r.CreatedByUser)
                .Include(r => r.Quiz)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteRoomAsync(Guid roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Participants)
                .Include(r => r.RoomAnswers)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return false;
            }

            // Delete all room answers associated with this room
            if (room.RoomAnswers.Any())
            {
                _context.RoomAnswers.RemoveRange(room.RoomAnswers);
            }

            // Delete all participants associated with this room
            if (room.Participants.Any())
            {
                _context.RoomParticipants.RemoveRange(room.Participants);
            }

            // Delete the room itself
            _context.Rooms.Remove(room);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task NotifyQuizEndedAsync(string roomCode)
        {
            // Send real-time update to all participants in the room that the quiz has ended
            await _hubContext.Clients.Group(roomCode).SendAsync("QuizEnded", roomCode);
        }

        public async Task<bool> RemoveParticipantFromRoomAsync(Guid roomId, int userIdToRemove, int currentUserId)
        {
            // Get the room to verify it exists and check if current user is the host
            var room = await _context.Rooms
                .Include(r => r.CreatedByUser) // Include the host user to get their name
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
            {
                return false;
            }

            // Check if the current user is the host (can remove any participant)
            if (room.CreatedBy != currentUserId)
            {
                return false; // Only host can remove participants
            }

            // Find the participant to remove
            var participantToRemove = await _context.RoomParticipants
                .Include(rp => rp.User) // Include the user to get their name
                .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userIdToRemove);
                
            if (participantToRemove == null)
            {
                return false; // Participant not found in the room
            }

            // Remove the participant
            _context.RoomParticipants.Remove(participantToRemove);
            await _context.SaveChangesAsync();

            // Get the user who was removed to send them a notification
            var removedUser = participantToRemove.User;

            // Send real-time update to the removed user to redirect them to home
            // Send directly to the user's personal group
            await _hubContext.Clients.Group($"User-{removedUser.Id}").SendAsync("UserRemovedFromRoom", room.Code, room.Name, room.CreatedByUser?.Name ?? "the host");

            // Send update to all other participants in the room that someone was removed
            await _hubContext.Clients.GroupExcept(room.Code, new[] { removedUser.Id.ToString() })
                .SendAsync("ParticipantRemoved", removedUser.Name, room.Code);

            return true;
        }
    }
}