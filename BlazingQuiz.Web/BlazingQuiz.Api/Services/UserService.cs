using BlazingQuiz.Api.Data;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class UserService
    {
        private readonly QuizContext _context;
        public UserService(QuizContext context) 
        {
            _context = context;
        }
        public async Task<PageResult<UserDto>> GetUserAsync(UserApprovedFilter approveType, int startIndex, int pageSize)
        {
            var query = _context.Users.Where(u => u.Role != nameof(UserRole.Admin)).AsQueryable();
            if (approveType != UserApprovedFilter.All)
            {
                if (approveType == UserApprovedFilter.ApprovedOnly)
                    query = query.Where(u => u.IsApproved);
                else
                    query = query.Where(u => !u.IsApproved);
            }
            var total = await query.CountAsync();
            var users = await query.OrderByDescending(u => u.Id)
                .Skip(startIndex)
                .Take(pageSize)
                .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Phone, u.IsApproved))
                .ToArrayAsync();
            return new PageResult<UserDto>(users, total);
        }

        public async Task ToggleUserApprovedStatusAsync(int userId)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (dbUser != null)
            {
                dbUser.IsApproved = !dbUser.IsApproved;
                await _context.SaveChangesAsync();
            }
        }
    }
}
