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
                _context.Quizzes.Add(quiz);
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

        public async Task<QuizListDto[]> GetQuizesAsync()
        {
            //TODO: Implement paging and server side filter (if required)
            return await _context.Quizzes.Select(q => new QuizListDto
            {
                Id = q.Id,
                Name = q.Name,
                CategoryName = q.Category.Name,
                TotalQuestions = q.TotalQuestions,
                TimeInMinutes = q.TimeInMinutes,
                IsActive = q.IsActive,
                CategoryId = q.CategoryId
            })
            .ToArrayAsync();
        }

        public async Task<QuestionDto[]> GetQuizQuestionsAsync(Guid quizId) =>
            await _context.Questions.Where(q => q.QuizId == quizId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text
                })
                .ToArrayAsync();

        public async Task<QuizSaveDto?> GetQuizToEditAsync(Guid quizId)
        {
            var quiz = await _context.Quizzes
                .Where(q => q.Id == quizId)
                .Select(qz => new QuizSaveDto
                {
                    Id = qz.Id,
                    Name = qz.Name,
                    CategoryId = qz.CategoryId,
                    TotalQuestions = qz.TotalQuestions,
                    TimeInMinutes = qz.TimeInMinutes,
                    IsActive = qz.IsActive,
                    Questions = qz.Questions.Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
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
