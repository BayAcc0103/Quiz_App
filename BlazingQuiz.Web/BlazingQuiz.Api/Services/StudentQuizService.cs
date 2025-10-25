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
            var query = _context.Quizzes
                .Include(q => q.CreatedByUser)
                .Include(q => q.Category)
                .Where(q => q.IsActive);

            if(categoryId > 0)
            {
                query = query.Where(q => q.CategoryId == categoryId);
            }
            var quizzes = await query
                .Select(q => new QuizListDto
                {
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                    Name = q.Name,
                    TimeInMinutes = q.TimeInMinutes,
                    TotalQuestions = q.Questions.Count,
                    Id = q.Id,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    CreatedByName = q.CreatedByUser != null ? q.CreatedByUser.Name : "Unknown"
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
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    TextAnswer = q.IsTextAnswer ? q.TextAnswer : null, // Only include text answer if it's a text answer question
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
                    OptionId = 0 // Initialize with a default value, will be updated on save
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
            Console.WriteLine($"SaveQuestionResponseAsync called - StudentQuizId: {dto.StudentQuizId}, QuestionId: {dto.QuestionId}, OptionId: {dto.OptionId}, TextAnswer: '{dto.TextAnswer}'");
            
            var studentQuiz = await _context.StudentQuizzes.AsTracking()
               .FirstOrDefaultAsync(s => s.Id == dto.StudentQuizId);
            if (studentQuiz == null)
            {
                Console.WriteLine("Student quiz not found");
                return QuizApiResponse.Failure("Student quiz not found");
            }
            if (studentQuiz.StudentId != studentId)
            {
                Console.WriteLine("Invalid request - student ID mismatch");
                return QuizApiResponse.Failure("Invalid request");
            }

            var studentQuizQuestion = await _context.StudentQuizQuestions.AsTracking()
                .FirstOrDefaultAsync(sqq => sqq.StudentQuizId == dto.StudentQuizId && sqq.QuestionId == dto.QuestionId);

            if (studentQuizQuestion == null)
            {
                Console.WriteLine("Student quiz question not found");
                return QuizApiResponse.Failure("Student quiz question not found.");
            }

            // Determine if this is a text answer question based on the question type from DB
            // This is more reliable than relying on the DTO content
            var questionInfo = await _context.Questions
                .Where(q => q.Id == dto.QuestionId)
                .Select(q => new { q.IsTextAnswer })
                .FirstOrDefaultAsync();
            
            var isTextAnswer = questionInfo?.IsTextAnswer == true;

            if (isTextAnswer)
            {
                // This is a text input question
                // Always save the text answer, even if it's empty or null
                studentQuizQuestion.TextAnswer = dto.TextAnswer;
                
                // Debug logging
                Console.WriteLine($"Saving text answer for question {dto.QuestionId}: '{dto.TextAnswer}'");
                
                // Check if the text answer matches the correct answer
                var correctTextAnswer = await _context.Questions
                    .Where(q => q.Id == dto.QuestionId)
                    .Select(q => q.TextAnswer)
                    .FirstOrDefaultAsync();
                
                // Debug logging
                Console.WriteLine($"Correct text answer for question {dto.QuestionId}: '{correctTextAnswer}'");
                
                // Compare text answers (case-insensitive and trimmed)
                // Handle null/empty cases correctly
                if (!string.IsNullOrWhiteSpace(correctTextAnswer) && 
                    !string.IsNullOrWhiteSpace(dto.TextAnswer))
                {
                    var trimmedCorrect = correctTextAnswer.Trim();
                    var trimmedAnswer = dto.TextAnswer.Trim();
                    
                    // Debug logging for troubleshooting
                    Console.WriteLine($"Comparing text answers - Correct: '{trimmedCorrect}' | Student: '{trimmedAnswer}'");
                    
                    if (string.Equals(trimmedCorrect, trimmedAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        studentQuiz.Total++; // Mark as correct
                        Console.WriteLine("Text answer matched - incrementing score");
                    }
                    else
                    {
                        Console.WriteLine("Text answer did not match");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping text answer comparison - Correct empty/null: {string.IsNullOrWhiteSpace(correctTextAnswer)} | Student empty/null: {string.IsNullOrWhiteSpace(dto.TextAnswer)}");
                }
            }
            else
            {
                // This is a multiple choice question
                studentQuizQuestion.OptionId = dto.OptionId;

                var isSelectedOptionCorrect = await _context.Options
                    .Where(o => o.QuestionId == dto.QuestionId && o.Id == dto.OptionId)
                    .Select(o => o.IsCorrect)
                    .FirstOrDefaultAsync();
                if (isSelectedOptionCorrect)
                {
                    studentQuiz.Total++;
                }
            }
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
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

        public async Task<QuizApiResponse<QuizResultDto>> GetQuizResultAsync(int studentQuizId, int studentId)
        {
            var studentQuiz = await _context.StudentQuizzes
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(sq => sq.StudentQuizQuestions)
                .FirstOrDefaultAsync(sq => sq.Id == studentQuizId);

            if (studentQuiz == null)
            {
                return QuizApiResponse<QuizResultDto>.Failure("Student quiz not found.");
            }

            if (studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse<QuizResultDto>.Failure("Invalid request.");
            }

            var quizResult = new QuizResultDto
            {
                Id = studentQuiz.Id,
                QuizName = studentQuiz.Quiz.Name,
                TotalQuestions = studentQuiz.Quiz.Questions.Count,
                CorrectAnswers = studentQuiz.Total,
                IncorrectAnswers = studentQuiz.Quiz.Questions.Count - studentQuiz.Total,
                Questions = studentQuiz.Quiz.Questions.Select(q => new QuizResultQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new QuizResultOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text
                    }).ToList(),
                    SelectedOptionId = studentQuiz.StudentQuizQuestions
                        .FirstOrDefault(sqq => sqq.QuestionId == q.Id)?.OptionId ?? 0,
                    SelectedTextAnswer = studentQuiz.StudentQuizQuestions
                        .FirstOrDefault(sqq => sqq.QuestionId == q.Id)?.TextAnswer,
                    CorrectOptionId = q.IsTextAnswer ? 0 : q.Options.FirstOrDefault(o => o.IsCorrect)?.Id ?? 0, // For text questions, there's no correct option ID
                    CorrectTextAnswer = q.IsTextAnswer ? q.TextAnswer : null
                }).ToList()
            };

            return QuizApiResponse<QuizResultDto>.Success(quizResult);
        }

        public async Task<QuizApiResponse<IEnumerable<QuestionDto>>> GetAllQuestionsForQuizAsync(int studentQuizId, int studentId)
        {
            // Verify that the student quiz belongs to the student
            var studentQuiz = await _context.StudentQuizzes
                .FirstOrDefaultAsync(s => s.Id == studentQuizId && s.StudentId == studentId);
            
            if (studentQuiz == null)
            {
                return QuizApiResponse<IEnumerable<QuestionDto>>.Failure("Student quiz not found or unauthorized access");
            }

            // Get all questions for the quiz
            var questions = await _context.Questions
                .Where(q => q.QuizId == studentQuiz.QuizId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    TextAnswer = q.IsTextAnswer ? q.TextAnswer : null, // Only include text answer if it's a text answer question
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                })
                .ToArrayAsync();

            return QuizApiResponse<IEnumerable<QuestionDto>>.Success(questions);
        }

        public async Task<QuizApiResponse> SaveRatingAndCommentAsync(QuizRatingCommentDto dto, int studentId)
        {
            // Verify that the student quiz belongs to the student
            var studentQuiz = await _context.StudentQuizzes
                .FirstOrDefaultAsync(s => s.Id == dto.StudentQuizId && s.StudentId == studentId);
            
            if (studentQuiz == null)
            {
                return QuizApiResponse.Failure("Student quiz not found or unauthorized access");
            }

            try
            {
                // Check if rating already exists for this quiz by this student
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.StudentId == studentId && r.QuizId == studentQuiz.QuizId);
                
                if (existingRating != null)
                {
                    // Update existing rating
                    existingRating.Score = dto.RatingScore;
                    existingRating.CreatedOn = DateTime.UtcNow;
                }
                else
                {
                    // Create new rating
                    var rating = new Rating
                    {
                        StudentId = studentId,
                        QuizId = studentQuiz.QuizId,
                        Score = dto.RatingScore,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Ratings.Add(rating);
                }

                // Check if comment already exists for this quiz by this student
                var existingComment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.StudentId == studentId && c.QuizId == studentQuiz.QuizId);
                
                if (existingComment != null)
                {
                    // Update existing comment if provided
                    if (!string.IsNullOrWhiteSpace(dto.CommentContent))
                    {
                        existingComment.Content = dto.CommentContent;
                        existingComment.CreatedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        // If no content provided, remove the existing comment
                        _context.Comments.Remove(existingComment);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(dto.CommentContent))
                {
                    // Create new comment
                    var comment = new Comment
                    {
                        StudentId = studentId,
                        QuizId = studentQuiz.QuizId,
                        Content = dto.CommentContent!,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Comments.Add(comment);
                }

                await _context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
            }
        }
    }
}
