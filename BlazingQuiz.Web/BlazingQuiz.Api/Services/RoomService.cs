using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class RoomService
    {
        private readonly QuizContext _context;

        public RoomService(QuizContext context)
        {
            _context = context;
        }

        public async Task<Room> CreateRoomAsync(string name, string? description, int createdBy, int maxParticipants = 50)
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
                .Where(r => r.CreatedBy == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}