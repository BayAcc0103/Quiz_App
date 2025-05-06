using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
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

        public async Task<QuizApiResponse<int>> StartQuizAsync(int studentId, Guid quizId)
        {
            try
            {
                var studentQuiz = new StudentQuiz
                {
                    QuizId = quizId,
                    StudentId = studentId,
                    Status = nameof(StudentQuizStatus.Started),
                    StartedOn = DateTime.UtcNow,
                };
                _context.StudentQuizzes.Add(studentQuiz);
                await _context.SaveChangesAsync();

                return QuizApiResponse<int>.Success(studentQuiz.Id);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<int>.Failure(ex.Message);
            }
        }
        public async Task<QuizApiResponse<QuestionDto?>> GetNextQuestionForQuizAsync(int studentQuizId, int studentId)
        {
            //TODO: Try to get the data in less number of database trips
            var studentQuiz = await _context.StudentQuizzes
                .Include(s => s.StudentQuizQuestions)
                .FirstOrDefaultAsync(s => s.Id == studentQuizId);
            if (studentQuiz == null)
            {
                return QuizApiResponse<QuestionDto?>.Failure("Student quiz not found");
            }
            if(studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse<QuestionDto?>.Failure("Invalid request");
            }
            var questionsServed = studentQuiz.StudentQuizQuestions
                .Select(s => s.QuestionId)
                .ToArray();
            var nextQuestion = await _context.Questions
                .Where(q => q.QuizId == studentQuiz.QuizId)
                .Where(q => !questionsServed.Contains(q.Id))
                .OrderBy(q => Guid.NewGuid())
                .Take(1)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            if (nextQuestion == null)
            {
                return QuizApiResponse<QuestionDto?>.Failure("No more questions available");
            }

            try
            {
                var studentQuizQuestion = new StudentQuizQuestion
                {
                    QuestionId = nextQuestion.Id,
                    StudentQuizId = studentQuizId,
                };
                _context.StudentQuizQuestions.Add(studentQuizQuestion);
                await _context.SaveChangesAsync();
                return QuizApiResponse<QuestionDto?>.Success(nextQuestion);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<QuestionDto?>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse> SaveQuestionResponseAsync(StudentQuizQuestionResponseDto dto, int studentId)
        {
            var studentQuiz = await _context.StudentQuizzes.AsTracking()
               .FirstOrDefaultAsync(s => s.Id == dto.StudentQuizId);
            if (studentQuiz == null)
            {
                return QuizApiResponse.Failure("Student quiz not found");
            }
            if (studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse.Failure("Invalid request");
            }
            var isSelectedOptionCorrect = await _context.Options
                .Where(o => o.QuestionId == dto.QuestionId && o.Id == dto.OptionId)
                .Select(o => o.IsCorrect)
                .FirstOrDefaultAsync();
            if (isSelectedOptionCorrect)
            {
                studentQuiz.Total++;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return QuizApiResponse.Failure(ex.Message);
                }
            }
            return QuizApiResponse.Success();
        }

        public async Task<QuizApiResponse> SubmitQuizAsync(int studentQuizId,int studentId) =>
            await CompleteQuizAsync(studentQuizId, DateTime.UtcNow, nameof(StudentQuizStatus.Completed), studentId);

        public async Task<QuizApiResponse> ExitQuizAsync(int studentQuizId, int studentId) =>
            await CompleteQuizAsync(studentQuizId, null, nameof(StudentQuizStatus.Exited), studentId);

        public async Task<QuizApiResponse> AutoSubmitQuizAsync(int studentQuizId, int studentId) =>
            await CompleteQuizAsync(studentQuizId, DateTime.UtcNow, nameof(StudentQuizStatus.AutoSubmitted), studentId);

        public async Task<QuizApiResponse> CompleteQuizAsync(int studentQuizId, DateTime? completedOn, string status, int studentId)
        {
            var studentQuiz = await _context.StudentQuizzes.AsTracking()
                .FirstOrDefaultAsync(s => s.Id == studentQuizId);
            if (studentQuiz == null)
            {
                return QuizApiResponse.Failure("Quiz does not exit");
            }
            if (studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse.Failure("Invalid request");
            }
            if (studentQuiz.CompletedOn.HasValue
                || studentQuiz.Status == nameof(StudentQuizStatus.Exited))
            {
                return QuizApiResponse.Failure("Quiz already completed");
            }
            try
            {
                studentQuiz.CompletedOn = completedOn;
                studentQuiz.Status = status;

                var studentQuizQuestions = await _context.StudentQuizQuestions
                    .Where(q => q.StudentQuizId == studentQuizId)
                    .ToListAsync();
                _context.StudentQuizQuestions.RemoveRange(studentQuizQuestions);
                await _context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
            }
        }

        public async Task<PageResult<StudentQuizDto>> GetStudentQuizesAsync(int studentId, int startIndex, int pageSize) 
        {
            var query = _context.StudentQuizzes.Where(q => q.StudentId == studentId);
            var count = await query.CountAsync();
            var quizes = await query.OrderByDescending(q => q.StartedOn)
                .Skip(startIndex)
                .Take(pageSize)
                .Select(q => new StudentQuizDto
                {
                    Id = q.Id,
                    QuizId = q.QuizId,
                    QuizName = q.Quiz.Name,
                    CategoryName = q.Quiz.Category.Name,
                    StartedOn = q.StartedOn,
                    CompletedOn = q.CompletedOn,
                    Status = q.Status,
                    Total = q.Total,
                })
                .ToArrayAsync();
            return new PageResult<StudentQuizDto>(quizes, count);

        }
    }
}
