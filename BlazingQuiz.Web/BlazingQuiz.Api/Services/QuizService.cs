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
                AnswerExplanation = q.AnswerExplanation,
                ImagePath = q.ImagePath,
                AudioPath = q.AudioPath,
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
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    TotalQuestions = dto.TotalQuestions,
                    TimeInMinutes = dto.TimeInMinutes,
                    IsActive = dto.IsActive,
                    CreatedBy = userId, // Set the creator ID
                    ImagePath = dto.ImagePath, // Set the image path
                    AudioPath = dto.AudioPath, // Set the audio path
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
                dbQuiz.Description = dto.Description;
                dbQuiz.CategoryId = dto.CategoryId;
                dbQuiz.TotalQuestions = dto.TotalQuestions;
                dbQuiz.TimeInMinutes = dto.TimeInMinutes;
                dbQuiz.IsActive = dto.IsActive;
                dbQuiz.ImagePath = dto.ImagePath; // Set the image path
                dbQuiz.AudioPath = dto.AudioPath; // Set the audio path
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
                Description = q.Description,
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
                    AudioPath = q.AudioPath,
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
                Description = qz.Description,
                CategoryId = qz.CategoryId,
                TotalQuestions = qz.TotalQuestions,
                TimeInMinutes = qz.TimeInMinutes,
                IsActive = qz.IsActive,
                ImagePath = qz.ImagePath,
                AudioPath = qz.AudioPath,
                Questions = qz.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerExplanation = q.AnswerExplanation,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
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

        public async Task<QuizApiResponse<TeacherQuizFeedbackDto>> GetQuizFeedbackAsync(Guid quizId, int userId, bool isAdmin)
        {
            try
            {
                // First, check if the quiz exists and if the user has permission to view it
                var quizQuery = _context.Quizzes.Where(q => q.Id == quizId);
                
                // If the user is not an admin, ensure they can only access feedback for their own quizzes
                if (!isAdmin)
                {
                    quizQuery = quizQuery.Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue);
                }
                
                var quizExists = await quizQuery.AnyAsync();
                if (!quizExists)
                {
                    return QuizApiResponse<TeacherQuizFeedbackDto>.Failure("Quiz not found or you don't have permission to access feedback.");
                }

                // Get all feedback for this quiz, ordered by most recent first
                var allFeedbacks = await _context.QuizFeedbacks
                    .Include(f => f.Student)
                    .Include(f => f.Quiz)
                    .Where(f => f.QuizId == quizId)
                    .OrderByDescending(f => f.CreatedOn)
                    .ToListAsync();

                // Convert feedbacks to separate ratings and comments for the DTO
                var ratings = allFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Score))
                    .Select(f => new TeacherQuizRatingDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        StudentName = f.Student.Name,
                        Score = f.Score ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        QuizId = f.QuizId,
                        QuizName = f.Quiz.Name
                    })
                    .ToList();

                var comments = allFeedbacks
                    .Where(f => !string.IsNullOrEmpty(f.Comment))
                    .Select(f => new TeacherQuizCommentDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        StudentName = f.Student.Name,
                        Content = f.Comment ?? string.Empty,
                        CreatedOn = f.CreatedOn,
                        QuizId = f.QuizId,
                        QuizName = f.Quiz.Name
                    })
                    .ToList();

                // Create combined feedback data
                var combinedFeedback = allFeedbacks
                    .Select(f => new TeacherQuizFeedbackItemDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        StudentName = f.Student.Name,
                        Score = f.Score,
                        Content = f.Comment,
                        IsCommentDeleted = f.IsCommentDeleted,
                        CreatedOn = f.CreatedOn,
                        QuizId = f.QuizId,
                        QuizName = f.Quiz.Name
                    })
                    .ToList();

                var feedback = new TeacherQuizFeedbackDto
                {
                    Ratings = ratings,
                    Comments = comments,
                    CombinedFeedback = combinedFeedback
                };

                return QuizApiResponse<TeacherQuizFeedbackDto>.Success(feedback);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<TeacherQuizFeedbackDto>.Failure(ex.Message);
            }
        }

        public async Task<bool> DeleteQuizFeedbackAsync(int feedbackId, int userId, bool isAdmin)
        {
            var feedback = await _context.QuizFeedbacks.FindAsync(feedbackId);
            if (feedback == null)
            {
                return false;
            }

            // If the user is not an admin, ensure they can only delete their own feedback
            // Actually, for this specific case, teachers are managing quiz feedback,
            // so we want to check that the teacher created the quiz this feedback is for
            if (!isAdmin)
            {
                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == feedback.QuizId);
                if (quiz?.CreatedBy != userId)
                {
                    return false; // Teacher didn't create this quiz, so can't delete its feedback
                }
            }

            _context.QuizFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRatingAsync(int feedbackId, int userId, bool isAdmin)
        {
            var feedback = await _context.QuizFeedbacks.FindAsync(feedbackId);
            if (feedback == null)
            {
                return false;
            }

            // Check if this feedback contains a rating (Score) to be deleted
            if (string.IsNullOrEmpty(feedback.Score))
            {
                return false; // This feedback doesn't contain a rating
            }

            // If the user is not an admin, ensure they can only delete ratings for quizzes they created
            if (!isAdmin)
            {
                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == feedback.QuizId);
                if (quiz?.CreatedBy != userId)
                {
                    return false; // Teacher didn't create this quiz, so can't delete its rating
                }
            }

            // For ratings, we'll also perform a soft delete by clearing the score
            // but keeping the record (in case it also has a comment)
            feedback.Score = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int feedbackId, int userId, bool isAdmin)
        {
            var feedback = await _context.QuizFeedbacks.FindAsync(feedbackId);
            if (feedback == null)
            {
                return false;
            }

            // If the user is not an admin, ensure they can only delete comments for quizzes they created
            if (!isAdmin)
            {
                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == feedback.QuizId);
                if (quiz?.CreatedBy != userId)
                {
                    return false; // Teacher didn't create this quiz, so can't delete its comment
                }
            }

            // Perform soft delete by marking the comment as deleted instead of removing it
            feedback.IsCommentDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
