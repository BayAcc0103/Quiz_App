using BlazingQuiz.Shared.DTOs;
using BlazingQuiz.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;

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
            var userData = await query.OrderByDescending(u => u.Id)
                .Skip(startIndex)
                .Take(pageSize)
                .Select(u => new
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsApproved = u.IsApproved,
                    AvatarPath = u.AvatarPath,
                    Role = u.Role ?? ""
                })
                .ToArrayAsync();

            var users = userData.Select(u => new UserDto(u.Id, u.Name, u.Email, u.Phone, u.IsApproved, u.AvatarPath, u.Role)).ToArray();
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

        public async Task<AdminQuizStudentListDto> GetQuizStudentsAsync(Guid quizId, int startIndex, int pageSize, bool fetchQuizInfo)
        {
            var result = new AdminQuizStudentListDto();
            using var context = _contextFactory.CreateDbContext();

            if (fetchQuizInfo)
            {
                var quizInfo = await context.Quizzes
                    .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                    .Where(q => q.Id == quizId)
                    .Select(q => new {
                        QuizName = q.Name,
                        CategoryName = q.CategoryId.HasValue ?
                            q.QuizCategories.Any(qc => qc.CategoryId == q.CategoryId) ?
                                q.QuizCategories.First(qc => qc.CategoryId == q.CategoryId).Category.Name :
                                "No Category" :
                            q.QuizCategories.Any() ? string.Join(", ", q.QuizCategories.Select(qc => qc.Category.Name)) : "No Category"
                    })
                    .FirstOrDefaultAsync();

                if(quizInfo == null)
                {
                    result.Students = new PageResult<AdminQuizStudentDto>([], 0);
                    return result;
                }


                result.QuizName = quizInfo.QuizName;
                result.CategoryName = quizInfo.CategoryName;
            }

            var query = context.StudentQuizzes
                    .Where(q => q.QuizId == quizId);

            var count = await query.CountAsync();

            var studentData = await query
                .OrderByDescending(s => s.StartedOn)
                .Skip(startIndex)
                .Take(pageSize)
                .Select(q => new
                {
                    Name = q.Student.Name,
                    StartedOn = q.StartedOn,
                    CompletedOn = q.CompletedOn,
                    Status = q.Status,
                    Total = q.Total
                })
                .ToArrayAsync();

            var students = studentData.Select(s => new AdminQuizStudentDto
            {
                Name = s.Name,
                StartedOn = s.StartedOn,
                CompletedOn = s.CompletedOn,
                Status = s.Status,
                Total = s.Total
            }).ToArray();

            var pageResult = new PageResult<AdminQuizStudentDto>(students, count);
            result.Students = pageResult;
            return result;
        }

        public async Task<QuizApiResponse> CreateUserAsync(RegisterDto dto)
        {
            using var context = _contextFactory.CreateDbContext();
            var passwordHasher = new PasswordHasher<User>();

            // Check if user with email already exists
            if(await context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return QuizApiResponse.Failure("Email already exists.");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role.ToString(),
                IsApproved = true // Admin-created users are automatically approved
            };

            user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
            context.Users.Add(user);

            try
            {
                await context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
            }
        }
    }
}
