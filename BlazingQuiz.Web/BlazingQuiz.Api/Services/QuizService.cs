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
        public async Task<QuizApiResponse> SaveQuizAsync(QuizSaveDto dto)
        {
            var questions = dto.Questions.Select(q => new Question
            {
                Id = q.Id,
                Text = q.Text,
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
                    Questions = questions
                };
            }
            else
            {
                // Update existing quiz
                var dbQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == dto.Id);
                if (dbQuiz == null)
                {
                    // Quiz does not exist, throw error or return some response
                    return QuizApiResponse.Failure("Quiz does not exist");
                }
                dbQuiz.Name = dto.Name;
                dbQuiz.CategoryId = dto.CategoryId;
                dbQuiz.TotalQuestions = dto.TotalQuestions;
                dbQuiz.TimeInMinutes = dto.TimeInMinutes;
                dbQuiz.IsActive = dto.IsActive;
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
    }
}
