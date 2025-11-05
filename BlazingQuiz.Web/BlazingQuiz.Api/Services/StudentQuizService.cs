using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using BlazingQuiz.Shared.Enums;
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
                    Description = q.Description,
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
                    StudentId = q.StudentId,
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
                // Always create new feedback (allow multiple ratings per student-quiz combination)
                var feedback = new QuizFeedback
                {
                    StudentId = studentId,
                    QuizId = studentQuiz.QuizId,
                    Score = ConvertRatingToText(dto),
                    Comment = dto.CommentContent,
                    CreatedOn = DateTime.UtcNow
                };
                _context.QuizFeedbacks.Add(feedback);

                await _context.SaveChangesAsync();
                return QuizApiResponse.Success();
            }
            catch (Exception ex)
            {
                return QuizApiResponse.Failure(ex.Message);
            }
        }

        private string ConvertRatingToText(QuizRatingCommentDto dto)
        {
            // If RatingText is provided and valid, use it after trimming
            if (!string.IsNullOrWhiteSpace(dto.RatingText))
            {
                var trimmedRatingText = dto.RatingText.Trim();
                if (BlazingQuiz.Shared.Enums.RatingText.IsValid(trimmedRatingText))
                {
                    return trimmedRatingText;
                }
                // If RatingText is provided but invalid, log potential issue and continue to RatingScore logic
                Console.WriteLine($"Invalid RatingText provided: '{dto.RatingText}', trimmed: '{trimmedRatingText}'");
            }
            
            // Otherwise, convert the RatingScore to text
            if (dto.RatingScore.HasValue)
            {
                return dto.RatingScore.Value switch
                {
                    1 => BlazingQuiz.Shared.Enums.RatingText.VeryBad,
                    2 => BlazingQuiz.Shared.Enums.RatingText.Bad,
                    3 => BlazingQuiz.Shared.Enums.RatingText.Normal,
                    4 => BlazingQuiz.Shared.Enums.RatingText.Good,
                    5 => BlazingQuiz.Shared.Enums.RatingText.VeryGood,
                    _ => BlazingQuiz.Shared.Enums.RatingText.Normal // Default value
                };
            }
            
            // Default to normal if no valid rating is provided
            return BlazingQuiz.Shared.Enums.RatingText.Normal;
        }

        public async Task<QuizApiResponse<QuizDetailsDto>> GetQuizDetailsAsync(Guid quizId)
        {
            try
            {
                // Get the quiz with its details
                var quiz = await _context.Quizzes
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.Category)
                    .Where(q => q.Id == quizId && q.IsActive)
                    .Select(q => new QuizDetailsDto
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Description = q.Description,
                        CategoryId = q.CategoryId,
                        CategoryName = q.Category.Name,
                        TotalQuestions = q.Questions.Count,
                        TimeInMinutes = q.TimeInMinutes,
                        IsActive = q.IsActive,
                        ImagePath = q.ImagePath,
                        AudioPath = q.AudioPath,
                        CreatedByName = q.CreatedByUser != null ? q.CreatedByUser.Name : "Unknown",
                        CreatedOn = DateTime.UtcNow // Using current time since User entity doesn't have a CreatedOn property
                    })
                    .FirstOrDefaultAsync();

                if (quiz == null)
                {
                    return QuizApiResponse<QuizDetailsDto>.Failure("Quiz not found or not active.");
                }

                // Get recent feedback for this quiz (limit to 3 most recent)
                var recentFeedbacks = await _context.QuizFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.QuizId == quizId)
                    .OrderByDescending(f => f.CreatedOn)
                    .Take(3)
                    .ToListAsync();

                // Convert feedbacks to separate ratings and comments for the DTO
                var recentRatings = recentFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Score))
                    .Select(f => new RatingDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        QuizId = f.QuizId,
                        Score = f.Score ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        StudentName = f.Student.Name
                    })
                    .ToList();

                var recentComments = recentFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Comment))
                    .Select(f => new CommentDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        QuizId = f.QuizId,
                        Content = f.Comment ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        StudentName = f.Student.Name
                    })
                    .ToList();

                quiz.RecentRatings = recentRatings.Take(1).ToList();
                quiz.RecentComments = recentComments.Take(1).ToList();

                return QuizApiResponse<QuizDetailsDto>.Success(quiz);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<QuizDetailsDto>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse<QuizAllFeedbackDto>> GetAllFeedbackAsync(Guid quizId)
        {
            try
            {
                // Get all feedback for this quiz, ordered by most recent first
                var allFeedbacks = await _context.QuizFeedbacks
                    .Include(f => f.Student)
                    .Where(f => f.QuizId == quizId)
                    .OrderByDescending(f => f.CreatedOn)
                    .ToListAsync();

                // Convert feedbacks to separate ratings and comments for the DTO
                var ratings = allFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Score))
                    .Select(f => new RatingDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        QuizId = f.QuizId,
                        Score = f.Score ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        StudentName = f.Student.Name
                    })
                    .ToList();

                // For students: Show comments that haven't been deleted
                var comments = allFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Comment) && !f.IsCommentDeleted)
                    .Select(f => new CommentDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        QuizId = f.QuizId,
                        Content = f.Comment ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        StudentName = f.Student.Name
                    })
                    .ToList();

                // Create combined feedback data for students - show both ratings and comments appropriately
                // If it's a deleted comment with no rating, hide it; if it's a deleted comment with a rating, show the rating but mark the comment appropriately
                var combinedFeedback = allFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Score) || !string.IsNullOrEmpty(f.Comment)) // Include entries that have either score or comment
                    .Where(f => !f.IsCommentDeleted || !string.IsNullOrEmpty(f.Score)) // Hide entries that are deleted comments with no rating
                    .Select(f => new CombinedFeedbackDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        StudentName = f.Student.Name,
                        Score = f.Score,
                        Comment = f.IsCommentDeleted ? "comment has been removed by teacher" : f.Comment, // Show placeholder for deleted comments that have ratings
                        CreatedOn = f.CreatedOn
                    })
                    .ToList();

                var allFeedback = new QuizAllFeedbackDto
                {
                    Ratings = ratings,
                    Comments = comments,
                    CombinedFeedback = combinedFeedback
                };

                return QuizApiResponse<QuizAllFeedbackDto>.Success(allFeedback);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<QuizAllFeedbackDto>.Failure(ex.Message);
            }
        }

        public async Task<StudentQuizDto[]> GetStudentQuizzesByQuizIdAsync(Guid quizId)
        {
            var studentQuizzes = await _context.StudentQuizzes
                .Include(sq => sq.Quiz)
                .Include(sq => sq.Student) // Include the student data
                .Where(sq => sq.QuizId == quizId && sq.Status == nameof(StudentQuizStatus.Completed))
                .Select(sq => new StudentQuizDto
                {
                    Id = sq.Id,
                    StudentId = sq.StudentId,
                    QuizId = sq.QuizId,
                    QuizName = sq.Quiz.Name,
                    CategoryName = sq.Quiz.Category.Name,
                    StartedOn = sq.StartedOn,
                    CompletedOn = sq.CompletedOn,
                    Status = sq.Status,
                    Total = sq.Total, // This contains the correct answers count
                })
                .ToArrayAsync();

            return studentQuizzes;
        }

        public async Task<BlazingQuiz.Shared.DTOs.StudentQuizQuestionResultDto[]> GetStudentQuizQuestionResponsesAsync(int studentQuizId, int studentId)
        {
            // Verify that the student quiz belongs to the student
            var studentQuiz = await _context.StudentQuizzes
                .FirstOrDefaultAsync(sq => sq.Id == studentQuizId && sq.StudentId == studentId);
            
            if (studentQuiz == null)
            {
                return Array.Empty<BlazingQuiz.Shared.DTOs.StudentQuizQuestionResultDto>(); // Return empty array if not authorized
            }

            var responses = await _context.StudentQuizQuestions
                .Where(sq => sq.StudentQuizId == studentQuizId)
                .Select(sq => new BlazingQuiz.Shared.DTOs.StudentQuizQuestionResultDto
                {
                    // StudentQuizQuestion doesn't have an Id property, so we can't use sq.Id
                    // We'll create a composite key from StudentQuizId and QuestionId for the ID
                    Id = (sq.StudentQuizId * 1000) + sq.QuestionId, // Create a unique ID using StudentQuizId and QuestionId
                    StudentQuizId = sq.StudentQuizId,
                    QuestionId = sq.QuestionId,
                    OptionId = sq.OptionId,
                    TextAnswer = sq.TextAnswer,
                    // Calculate approximate answered time based on the quiz start time
                    // Since we don't track exact answer time per question, we'll use a simple approach
                    // Create a unique value based on both StudentQuizId and QuestionId to distribute times for different questions
                    AnsweredAt = sq.StudentQuiz.StartedOn.AddSeconds(Math.Abs((sq.StudentQuizId * 17) + sq.QuestionId) % 300) // Distribute times within the quiz duration 
                })
                .ToArrayAsync();

            return responses;
        }
    }
}
