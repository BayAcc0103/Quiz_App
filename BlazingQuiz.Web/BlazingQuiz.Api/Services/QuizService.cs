using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class QuizService
    {
        private readonly QuizContext _context;

        public QuizService(QuizContext context)
        {
            _context = context;
        }
        public async Task<QuizApiResponse> SaveQuizAsync(QuizSaveDto dto, int userId)
        {
            var questions = dto.Questions.Select(q => new Question
            {
                Id = q.Id,
                Text = q.Text,
                ImagePath = q.ImagePath,
                IsTextAnswer = q.IsTextAnswer,
                TextAnswer = q.TextAnswer,
                Options = q.Options.Select(o => new Option
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToArray()
            }).ToArray();
            if (dto.Id == Guid.Empty)
            {
                var quiz = new Quiz
                {
                    Name = dto.Name,
                    CategoryId = dto.CategoryId,
                    TotalQuestions = dto.TotalQuestions,
                    TimeInMinutes = dto.TimeInMinutes,
                    IsActive = dto.IsActive,
                    CreatedBy = userId, // Set the creator ID
                    ImagePath = dto.ImagePath, // Set the image path
                    Questions = questions
                };
                _context.Quizzes.Add(quiz);
            }
            else
            {
                // Update existing quiz - check if the current user is the creator (if they're not an admin)
                var dbQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == dto.Id);
                if (dbQuiz == null)
                {
                    // Quiz does not exist, throw error or return some response
                    return QuizApiResponse.Failure("Quiz does not exist");
                }
                
                // Check if the user is the creator of this quiz (skip for admin)
                if (dbQuiz.CreatedBy.HasValue && dbQuiz.CreatedBy != userId)
                {
                    return QuizApiResponse.Failure("You can only edit quizzes that you created.");
                }
                
                dbQuiz.Name = dto.Name;
                dbQuiz.CategoryId = dto.CategoryId;
                dbQuiz.TotalQuestions = dto.TotalQuestions;
                dbQuiz.TimeInMinutes = dto.TimeInMinutes;
                dbQuiz.IsActive = dto.IsActive;
                dbQuiz.ImagePath = dto.ImagePath; // Set the image path
                dbQuiz.Questions = questions;
            }

            try
            {
                await _context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                // Handle concurrency exception
                return QuizApiResponse.Failure(ex.Message);
            }
        }

        public async Task<QuizListDto[]> GetQuizesAsync(int userId, bool isAdmin)
        {
            //TODO: Implement paging and server side filter (if required)
            IQueryable<Quiz> query = _context.Quizzes;
            
            // If the user is not an admin, only return quizzes created by this user
            if (!isAdmin)
            {
                query = query.Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue);
            }
            
            return await query.Select(q => new QuizListDto
            {
                Id = q.Id,
                Name = q.Name,
                CategoryName = q.Category.Name,
                TotalQuestions = q.TotalQuestions,
                TimeInMinutes = q.TimeInMinutes,
                IsActive = q.IsActive,
                CategoryId = q.CategoryId,
                ImagePath = q.ImagePath
            })
            .ToArrayAsync();
        }

        public async Task<QuestionDto[]> GetQuizQuestionsAsync(Guid quizId) =>
            await _context.Questions.Where(q => q.QuizId == quizId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    ImagePath = q.ImagePath,
                    IsTextAnswer = q.IsTextAnswer,
                    TextAnswer = q.TextAnswer,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                })
                .ToArrayAsync();

        public async Task<QuizSaveDto?> GetQuizToEditAsync(Guid quizId, int userId, bool isAdmin)
        {
            var query = _context.Quizzes.Where(q => q.Id == quizId);
            
            // If the user is not an admin, ensure they can only access their own quizzes
            if (!isAdmin)
            {
                query = query.Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue);
            }
            
            var quiz = await query.Select(qz => new QuizSaveDto
            {
                Id = qz.Id,
                Name = qz.Name,
                CategoryId = qz.CategoryId,
                TotalQuestions = qz.TotalQuestions,
                TimeInMinutes = qz.TimeInMinutes,
                IsActive = qz.IsActive,
                ImagePath = qz.ImagePath,
                Questions = qz.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    ImagePath = q.ImagePath,
                    IsTextAnswer = q.IsTextAnswer,
                    TextAnswer = q.TextAnswer,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            }).FirstOrDefaultAsync();
            
            return quiz;
        }
    }
}
