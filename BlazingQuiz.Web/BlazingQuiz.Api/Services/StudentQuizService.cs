using BlazingQuiz.Api.Data;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class StudentQuizService
    {
        private readonly QuizContext _context;

        public StudentQuizService(QuizContext context)
        {
            _context = context;
        }
        public async Task<QuizListDto[]> GetActiveQuizesAsync(int categoryId)
        {
            var query = _context.Quizzes.Where(q => q.IsActive);

            if(categoryId > 0)
            {
                query = query.Where(q => q.CategoryId == categoryId);
            }
            var quizzes = await query
                .Select(q => new QuizListDto
                {
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                    Name = q.Category.Name,
                    TimeInMinutes = q.TimeInMinutes,
                    TotalQuestions = q.Questions.Count,
                    Id = q.Id,
                })
                .ToArrayAsync();
            return quizzes;
        }
    }
}
