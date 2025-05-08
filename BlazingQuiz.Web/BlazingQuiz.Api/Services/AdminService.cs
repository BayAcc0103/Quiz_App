using BlazingQuiz.Shared.DTOs;
using BlazingQuiz.Shared;
using Microsoft.EntityFrameworkCore;
using BlazingQuiz.Api.Data;

namespace BlazingQuiz.Api.Services
{
    public class AdminService
    {
        private readonly IDbContextFactory<QuizContext> _contextFactory;

        public AdminService(IDbContextFactory<QuizContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<AdminHomeDataDto> GetAdminHomeDataAsync()
        {
            var totalCategoriesTask =  _contextFactory.CreateDbContext().Categories.CountAsync();
            var totalStudentsTask = _contextFactory.CreateDbContext().Users.Where(u => u.Role == nameof(UserRole.Student)).CountAsync();
            var approvedStudentsTask = _contextFactory.CreateDbContext().Users.Where(u => u.Role == nameof(UserRole.Student) && u.IsApproved).CountAsync();
            var totalQuizesTask =  _contextFactory.CreateDbContext().Quizzes.CountAsync();
            var activeQuizesTask = _contextFactory.CreateDbContext().Quizzes.Where(q => q.IsActive).CountAsync();

            var totalCategories = await totalCategoriesTask;
            var totalStudents = await totalStudentsTask;
            var approvedStudents = await approvedStudentsTask;
            var totalQuizes = await totalQuizesTask;
            var activeQuizes = await activeQuizesTask;

            return new AdminHomeDataDto(totalCategories, totalStudents, totalQuizes, approvedStudents, activeQuizes);
        }

        public async Task<PageResult<UserDto>> GetUserAsync(UserApprovedFilter approveType, int startIndex, int pageSize)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Users.Where(u => u.Role != nameof(UserRole.Admin)).AsQueryable();
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
            using var context = _contextFactory.CreateDbContext();
            var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (dbUser != null)
            {
                dbUser.IsApproved = !dbUser.IsApproved;
                await context.SaveChangesAsync();
            }
        }
    }
}
