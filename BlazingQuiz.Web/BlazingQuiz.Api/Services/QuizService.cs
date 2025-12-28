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
            if (dto.Id == Guid.Empty)
            {
                // Creating a new quiz
                var questions = dto.Questions.Select(q => new Question
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerExplanation = q.AnswerExplanation,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new Option
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToArray(),
                    TextAnswers = q.TextAnswers.Select(ta => new TextAnswer
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToArray()
                }).ToArray();

                var quiz = new Quiz
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CategoryId = dto.CategoryIds != null && dto.CategoryIds.Any() ? dto.CategoryIds.First() : (int?)null, // Keep first as primary for backward compatibility
                    TotalQuestions = dto.TotalQuestions,
                    TimeInMinutes = dto.TimeInMinutes,
                    IsActive = dto.IsActive,
                    IsBan = false, // New quizzes are not banned by default
                    Level = dto.Level,
                    CreatedBy = userId,
                    ImagePath = dto.ImagePath,
                    AudioPath = dto.AudioPath,
                    CreatedAt = DateTime.UtcNow,
                    Questions = questions
                };
                _context.Quizzes.Add(quiz);
                if (dto.CategoryIds != null && dto.CategoryIds.Any())
                {
                    var quizCategories = dto.CategoryIds.Select(catId => new QuizCategory
                    {
                        Quiz = quiz, // Use the quiz object directly instead of quiz.Id
                        CategoryId = catId
                    }).ToList();
                    _context.QuizCategories.AddRange(quizCategories);
                }
            }
            else
            {
                // Editing existing quiz - check if the current user is the creator (if they're not an admin)
                var dbQuiz = await _context.Quizzes
                    .Include(q => q.QuizCategories) // Include existing quiz-category relationships
                    .Include(q => q.Questions)
                        .ThenInclude(qq => qq.Options)
                    .Include(q => q.Questions)
                        .ThenInclude(qq => qq.TextAnswers)
                    .FirstOrDefaultAsync(q => q.Id == dto.Id);
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

                // Determine if questions/answers have changed by comparing with the existing quiz
                bool questionsOrAnswersChanged = QuestionsOrAnswersChanged(dbQuiz, dto);

                if (questionsOrAnswersChanged)
                {
                    // Create a new quiz with updated questions/answers and set old quiz as inactive
                    var newQuiz = new Quiz
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        CategoryId = dto.CategoryIds != null && dto.CategoryIds.Any() ? dto.CategoryIds.First() : (int?)null, // Keep first as primary for backward compatibility
                        TotalQuestions = dto.TotalQuestions,
                        TimeInMinutes = dto.TimeInMinutes,
                        IsActive = dto.IsActive,
                        IsBan = dbQuiz.IsBan, // Preserve the ban status
                        Level = dto.Level,
                        CreatedBy = userId,
                        ImagePath = dto.ImagePath,
                        AudioPath = dto.AudioPath,
                        CreatedAt = DateTime.UtcNow,
                        Questions = dto.Questions.Select(q => new Question
                        {
                            Text = q.Text,
                            AnswerExplanation = q.AnswerExplanation,
                            ImagePath = q.ImagePath,
                            AudioPath = q.AudioPath,
                            IsTextAnswer = q.IsTextAnswer,
                            Options = q.Options.Select(o => new Option
                            {
                                Text = o.Text,
                                IsCorrect = o.IsCorrect
                            }).ToArray(),
                            TextAnswers = q.TextAnswers.Select(ta => new TextAnswer
                            {
                                Text = ta.Text
                            }).ToArray()
                        }).ToArray()
                    };

                    _context.Quizzes.Add(newQuiz);

                    // Add category relationships for the new quiz
                    if (dto.CategoryIds != null && dto.CategoryIds.Any())
                    {
                        var quizCategories = dto.CategoryIds.Select(catId => new QuizCategory
                        {
                            Quiz = newQuiz,
                            CategoryId = catId
                        }).ToList();
                        _context.QuizCategories.AddRange(quizCategories);
                    }

                    // Save the new quiz first to get its ID
                    await _context.SaveChangesAsync();

                    // Set the old quiz as inactive
                    dbQuiz.IsActive = false;
                    dbQuiz.IsBan = true; // Also ban the old quiz when creating a new version

                    // Copy bookmarks from old quiz to new quiz
                    await CopyBookmarksToNewQuiz(dbQuiz.Id, newQuiz.Id);

                    // Update the quiz ID in the DTO to return the new quiz ID
                    dto.Id = newQuiz.Id;
                }
                else
                {
                    // Only quiz information changed, update the existing quiz
                    dbQuiz.Name = dto.Name;
                    dbQuiz.Description = dto.Description;
                    dbQuiz.CategoryId = dto.CategoryIds != null && dto.CategoryIds.Any() ? dto.CategoryIds.First() : (int?)null; // Keep first as primary for backward compatibility
                    dbQuiz.TotalQuestions = dto.TotalQuestions;
                    dbQuiz.TimeInMinutes = dto.TimeInMinutes;
                    dbQuiz.IsActive = dto.IsActive;
                    dbQuiz.Level = dto.Level; // Set the level
                    dbQuiz.ImagePath = dto.ImagePath; // Set the image path
                    dbQuiz.AudioPath = dto.AudioPath; // Set the audio path

                    // Update quiz-category relationships
                    if (dbQuiz.QuizCategories != null)
                    {
                        _context.QuizCategories.RemoveRange(dbQuiz.QuizCategories);
                    }

                    if (dto.CategoryIds != null && dto.CategoryIds.Any())
                    {
                        var quizCategories = dto.CategoryIds.Select(catId => new QuizCategory
                        {
                            QuizId = dbQuiz.Id,
                            CategoryId = catId
                        }).ToList();

                        _context.QuizCategories.AddRange(quizCategories);
                    }
                }
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

        private bool QuestionsOrAnswersChanged(Quiz existingQuiz, QuizSaveDto newQuizDto)
        {
            // Compare questions and their options/answers
            if (existingQuiz.Questions.Count() != newQuizDto.Questions.Count())
            {
                return true;
            }

            // Create dictionaries for quick lookup by question text (or other unique identifier)
            var existingQuestionsDict = existingQuiz.Questions.ToDictionary(q => q.Id, q => q);
            var newQuestionsDict = newQuizDto.Questions.ToDictionary(q => q.Id, q => q);

            foreach (var newQuestion in newQuizDto.Questions)
            {
                if (!existingQuestionsDict.ContainsKey(newQuestion.Id))
                {
                    // New question added
                    return true;
                }

                var existingQuestion = existingQuestionsDict[newQuestion.Id];

                // Compare question properties
                if (existingQuestion.Text != newQuestion.Text ||
                    existingQuestion.AnswerExplanation != newQuestion.AnswerExplanation ||
                    existingQuestion.ImagePath != newQuestion.ImagePath ||
                    existingQuestion.AudioPath != newQuestion.AudioPath ||
                    existingQuestion.IsTextAnswer != newQuestion.IsTextAnswer)
                {
                    return true;
                }

                // Compare options if it's not a text answer question
                if (!newQuestion.IsTextAnswer)
                {
                    if (existingQuestion.Options.Count() != newQuestion.Options.Count())
                    {
                        return true;
                    }

                    var existingOptionsDict = existingQuestion.Options.ToDictionary(o => o.Id, o => o);
                    var newOptionsDict = newQuestion.Options.ToDictionary(o => o.Id, o => o);

                    foreach (var newOption in newQuestion.Options)
                    {
                        if (!existingOptionsDict.ContainsKey(newOption.Id))
                        {
                            // New option added
                            return true;
                        }

                        var existingOption = existingOptionsDict[newOption.Id];
                        if (existingOption.Text != newOption.Text || existingOption.IsCorrect != newOption.IsCorrect)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // Compare text answers if it's a text answer question
                    if (existingQuestion.TextAnswers.Count() != newQuestion.TextAnswers.Count())
                    {
                        return true;
                    }

                    var existingTextAnswersDict = existingQuestion.TextAnswers.ToDictionary(ta => ta.Id, ta => ta);
                    var newTextAnswersDict = newQuestion.TextAnswers.ToDictionary(ta => ta.Id, ta => ta);

                    foreach (var newTextAnswer in newQuestion.TextAnswers)
                    {
                        if (!existingTextAnswersDict.ContainsKey(newTextAnswer.Id))
                        {
                            // New text answer added
                            return true;
                        }

                        var existingTextAnswer = existingTextAnswersDict[newTextAnswer.Id];
                        if (existingTextAnswer.Text != newTextAnswer.Text)
                        {
                            return true;
                        }
                    }
                }
            }

            // Check if any questions were removed
            if (existingQuestionsDict.Keys.Except(newQuestionsDict.Keys).Any())
            {
                return true;
            }

            return false;
        }

        private async Task CopyBookmarksToNewQuiz(Guid oldQuizId, Guid newQuizId)
        {
            // Get all bookmarks for the old quiz
            var oldBookmarks = await _context.QuizBookmarks
                .Where(b => b.QuizId == oldQuizId)
                .ToListAsync();

            if (oldBookmarks.Any())
            {
                // Get the new quiz details to use its name and category
                var newQuizDetails = await _context.Quizzes
                    .Where(q => q.Id == newQuizId)
                    .Select(q => new {
                        Name = q.Name,
                        CategoryName = q.CategoryId.HasValue ?
                            q.QuizCategories.Any(qc => qc.CategoryId == q.CategoryId) ?
                                q.QuizCategories.First(qc => qc.CategoryId == q.CategoryId).Category.Name :
                                q.QuizCategories.Any() ? q.QuizCategories.First().Category.Name : "No Category" :
                            q.QuizCategories.Any() ? q.QuizCategories.First().Category.Name : "No Category"
                    })
                    .FirstOrDefaultAsync();

                // Create new bookmarks for the new quiz
                var newBookmarks = oldBookmarks.Select(oldBookmark => new QuizBookmark
                {
                    UserId = oldBookmark.UserId,
                    QuizId = newQuizId,
                    QuizName = newQuizDetails?.Name ?? oldBookmark.QuizName, // Use new quiz name if available
                    CategoryName = newQuizDetails?.CategoryName ?? oldBookmark.CategoryName, // Use new category name if available
                    BookmarkedOn = DateTime.UtcNow
                }).ToList();

                _context.QuizBookmarks.AddRange(newBookmarks);
            }
        }

        public async Task<QuizListDto[]> GetQuizesAsync(int userId, bool isAdmin)
        {
            //TODO: Implement paging and server side filter (if required)
            IQueryable<Quiz> query = _context.Quizzes
                .Include(q => q.QuizCategories)
                .ThenInclude(qc => qc.Category);

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
                CategoryName = q.CategoryId.HasValue ?
                    q.QuizCategories.Any(qc => qc.CategoryId == q.CategoryId) ?
                        q.QuizCategories.First(qc => qc.CategoryId == q.CategoryId).Category.Name :
                        q.QuizCategories.Any() ? q.QuizCategories.First().Category.Name : "No Category" :
                    q.QuizCategories.Any() ? q.QuizCategories.First().Category.Name : "No Category",
                TotalQuestions = q.TotalQuestions,
                TimeInMinutes = q.TimeInMinutes,
                IsActive = q.IsActive,
                IsBan = q.IsBan, // Include the ban status
                Level = q.Level, // Include the level
                CategoryId = q.CategoryId,
                ImagePath = q.ImagePath,
                CreatedBy = q.CreatedBy, // Include the creator ID
                CreatedAt = q.CreatedAt,
                AllCategoryNames = q.QuizCategories.Select(qc => qc.Category.Name).ToList()
            })
            .ToArrayAsync();
        }

        public async Task<QuestionDto[]> GetQuizQuestionsAsync(Guid quizId)
        {
            return await _context.Questions.Where(q => q.QuizId == quizId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerExplanation = q.AnswerExplanation,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList(),
                    TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToList() : new List<TextAnswerDto>()
                })
                .ToArrayAsync();
        }

        public async Task<QuizSaveDto?> GetQuizToEditAsync(Guid quizId, int userId, bool isAdmin)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizCategories)
                .Where(q => q.Id == quizId)

                // If the user is not an admin, ensure they can only access their own quizzes
                .Where(q => isAdmin || (q.CreatedBy == userId && q.CreatedBy.HasValue))
                .Select(qz => new QuizSaveDto
                {
                    Id = qz.Id,
                    Name = qz.Name,
                    Description = qz.Description,
                    CategoryIds = qz.QuizCategories != null ? qz.QuizCategories.Select(qc => qc.CategoryId).ToList() : new List<int>(),
                    TotalQuestions = qz.TotalQuestions,
                    TimeInMinutes = qz.TimeInMinutes,
                    IsActive = qz.IsActive,
                    Level = qz.Level, // Include the level
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
                        Options = q.Options.Select(o => new OptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect
                        }).ToList(),
                        TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                        {
                            Id = ta.Id,
                            Text = ta.Text
                        }).ToList() : new List<TextAnswerDto>()
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
                    .Where(f => f.Score.HasValue)
                    .Select(f => new TeacherQuizRatingDto
                    {
                        Id = f.Id,
                        StudentId = f.StudentId,
                        StudentName = f.Student.Name,
                        Score = f.ScoreText ?? string.Empty, // Use ScoreText property to get the string representation
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
                        Score = f.ScoreText, // Use ScoreText property to get the string representation
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
            if (!feedback.Score.HasValue)
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

            // For ratings, we'll also perform a soft delete by ...
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

        public async Task<bool> DeleteOptionAsync(int optionId, int userId, bool isAdmin)
        {
            var option = await _context.Options
                .Include(o => o.Question)
                .ThenInclude(q => q.Quiz)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (option == null)
            {
                return false;
            }

            // If the user is not an admin, ensure they can only delete options for quizzes they created
            if (!isAdmin)
            {
                if (option.Question.Quiz.CreatedBy != userId)
                {
                    return false; // Teacher didn't create this quiz, so can't delete its option
                }
            }

            _context.Options.Remove(option);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(int questionId, int userId, bool isAdmin)
        {
            var question = await _context.Questions
                .Include(q => q.Quiz)
                .Include(q => q.Options)
                .Include(q => q.TextAnswers)
                .Include(q => q.StudentQuizQuestions)
                .Include(q => q.StudentQuizQuestionsForRoom)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return false;
            }

            // If the user is not an admin, ensure they can only delete questions they created
            if (!isAdmin)
            {
                if (question.CreatedBy != userId)
                {
                    return false; // Teacher can only delete questions they created
                }
            }

            // Delete all related data due to foreign key constraints
            // First, delete related student quiz questions
            _context.StudentQuizQuestions.RemoveRange(question.StudentQuizQuestions);
            _context.StudentQuizQuestionsForRoom.RemoveRange(question.StudentQuizQuestionsForRoom);

            // Then delete related room answers that reference this question
            var roomAnswers = await _context.RoomAnswers.Where(ra => ra.QuestionId == questionId).ToListAsync();
            _context.RoomAnswers.RemoveRange(roomAnswers);

            // Then delete options and text answers
            _context.Options.RemoveRange(question.Options);
            _context.TextAnswers.RemoveRange(question.TextAnswers);

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizAsync(Guid quizId, int userId, bool isAdmin)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.TextAnswers)
                .Include(q => q.QuizCategories)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return false;
            }

            // If the user is not an admin, ensure they can only delete quizzes they created
            if (!isAdmin)
            {
                if (quiz.CreatedBy != userId)
                {
                    return false; // Teacher didn't create this quiz, so can't delete it
                }
            }

            // Delete all related records in the following tables:
            // 1. QuizBookmark - bookmarks for this quiz
            var quizBookmarks = await _context.QuizBookmarks.Where(b => b.QuizId == quizId).ToListAsync();
            _context.QuizBookmarks.RemoveRange(quizBookmarks);

            // 2. QuizFeedback - feedback for this quiz
            var quizFeedbacks = await _context.QuizFeedbacks.Where(f => f.QuizId == quizId).ToListAsync();
            _context.QuizFeedbacks.RemoveRange(quizFeedbacks);

            // 3. Room - rooms that use this quiz (and their related data)
            var rooms = await _context.Rooms.Where(r => r.QuizId == quizId).ToListAsync();
            foreach (var room in rooms)
            {
                // Delete RoomAnswers associated with these rooms
                var roomAnswers = await _context.RoomAnswers.Where(ra => ra.RoomId == room.Id).ToListAsync();
                _context.RoomAnswers.RemoveRange(roomAnswers);
            }
            _context.Rooms.RemoveRange(rooms);

            // 4. StudentQuizzes - student attempts for this quiz
            var studentQuizzes = await _context.StudentQuizzes.Where(sq => sq.QuizId == quizId).ToListAsync();
            _context.StudentQuizzes.RemoveRange(studentQuizzes);

            // 5. StudentQuizzesForRoom - student attempts for this quiz in rooms
            var studentQuizzesForRoom = await _context.StudentQuizzesForRoom.Where(sqfr => sqfr.QuizId == quizId).ToListAsync();
            _context.StudentQuizzesForRoom.RemoveRange(studentQuizzesForRoom);

            // Delete all related data due to foreign key constraints
            foreach (var question in quiz.Questions)
            {
                _context.Options.RemoveRange(question.Options);
                _context.TextAnswers.RemoveRange(question.TextAnswers);
            }
            _context.Questions.RemoveRange(quiz.Questions);
            _context.QuizCategories.RemoveRange(quiz.QuizCategories);

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TeacherQuizStudentListDto> GetQuizStudentsAsync(Guid quizId, int startIndex, int pageSize, bool fetchQuizInfo, int userId, bool isAdmin)
        {
            var result = new TeacherQuizStudentListDto();

            // First, check if the user has permission to access this quiz
            var quizQuery = _context.Quizzes.Where(q => q.Id == quizId);

            // If the user is not an admin, ensure they can only access their own quizzes
            if (!isAdmin)
            {
                quizQuery = quizQuery.Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue);
            }

            var quizExists = await quizQuery.AnyAsync();
            if (!quizExists)
            {
                return null; // Quiz doesn't exist or user doesn't have permission
            }

            if (fetchQuizInfo)
            {
                var quizInfo = await quizQuery
                    .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                    .Select(q => new {
                        QuizName = q.Name,
                        CategoryName = q.CategoryId.HasValue ?
                            q.QuizCategories.Any(qc => qc.CategoryId == q.CategoryId) ?
                                q.QuizCategories.First(qc => qc.CategoryId == q.CategoryId).Category.Name :
                                "No Category" :
                            q.QuizCategories.Any() ? string.Join(", ", q.QuizCategories.Select(qc => qc.Category.Name)) : "No Category"
                    })
                    .FirstOrDefaultAsync();

                if (quizInfo == null)
                {
                    result.Students = new PageResult<TeacherQuizStudentDto>([], 0);
                    return result;
                }

                result.QuizName = quizInfo.QuizName;
                result.CategoryName = quizInfo.CategoryName;
            }

            var query = _context.StudentQuizzes
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

            var students = studentData.Select(s => new TeacherQuizStudentDto
            {
                Name = s.Name,
                StartedOn = s.StartedOn,
                CompletedOn = s.CompletedOn,
                Status = s.Status,
                Total = s.Total
            }).ToArray();

            var pageResult = new PageResult<TeacherQuizStudentDto>(students, count);
            result.Students = pageResult;
            return result;
        }

        public async Task<QuestionDto[]> GetQuestionsAsync(int userId, bool isAdmin)
        {
            IQueryable<Question> query = _context.Questions
                .Include(q => q.Options)
                .Include(q => q.TextAnswers)
                .Include(q => q.Quiz);

            // If the user is not an admin, only return questions created by this user
            if (!isAdmin)
            {
                query = query.Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue);
            }

            return await query.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                AnswerExplanation = q.AnswerExplanation,
                ImagePath = q.ImagePath,
                AudioPath = q.AudioPath,
                IsTextAnswer = q.IsTextAnswer,
                Options = q.Options.Select(o => new OptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList(),
                TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                {
                    Id = ta.Id,
                    Text = ta.Text
                }).ToList() : new List<TextAnswerDto>(),
                CreatedAt = q.CreatedAt,
                CreatedBy = q.CreatedBy
            })
            .ToArrayAsync();
        }

        public async Task<QuestionDto[]> GetQuestionsCreatedByAsync(int userId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.TextAnswers)
                .Include(q => q.Quiz)
                .Where(q => q.CreatedBy == userId && q.CreatedBy.HasValue)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerExplanation = q.AnswerExplanation,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList(),
                    TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToList() : new List<TextAnswerDto>(),
                    CreatedAt = q.CreatedAt,
                    CreatedBy = q.CreatedBy
                })
                .ToArrayAsync();
        }

        public async Task<QuestionDto> GetQuestionByIdAsync(int questionId)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.TextAnswers)
                .Include(q => q.Quiz)
                .Where(q => q.Id == questionId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    AnswerExplanation = q.AnswerExplanation,
                    ImagePath = q.ImagePath,
                    AudioPath = q.AudioPath,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList(),
                    TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToList() : new List<TextAnswerDto>(),
                    CreatedAt = q.CreatedAt,
                    CreatedBy = q.CreatedBy
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SaveQuestionAsync(QuestionDto questionDto, int userId)
        {
            try
            {
                // Create a new Question entity from the DTO
                var question = new Question
                {
                    Text = questionDto.Text,
                    AnswerExplanation = questionDto.AnswerExplanation,
                    ImagePath = questionDto.ImagePath,
                    AudioPath = questionDto.AudioPath,
                    IsTextAnswer = questionDto.IsTextAnswer,
                    CreatedBy = userId, // Set the creator
                    CreatedAt = DateTime.UtcNow, // Set creation timestamp
                    Options = questionDto.Options?.Select(o => new Option
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToArray() ?? Array.Empty<Option>(),
                    TextAnswers = questionDto.TextAnswers?.Select(ta => new TextAnswer
                    {
                        Text = ta.Text
                    }).ToArray() ?? Array.Empty<TextAnswer>()
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving question: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateQuestionAsync(int questionId, QuestionDto questionDto, int userId, bool isAdmin)
        {
            try
            {
                // Find the existing question
                var existingQuestion = await _context.Questions
                    .Include(q => q.Options)
                    .Include(q => q.TextAnswers)
                    .FirstOrDefaultAsync(q => q.Id == questionId);

                if (existingQuestion == null)
                {
                    return false;
                }

                // Check if the user is the creator of this question (skip for admin)
                if (!isAdmin && existingQuestion.CreatedBy != userId)
                {
                    return false; // User can only update questions they created
                }

                // Update the question properties
                existingQuestion.Text = questionDto.Text;
                existingQuestion.AnswerExplanation = questionDto.AnswerExplanation;
                existingQuestion.ImagePath = questionDto.ImagePath;
                existingQuestion.AudioPath = questionDto.AudioPath;
                existingQuestion.IsTextAnswer = questionDto.IsTextAnswer;

                // Update options
                if (questionDto.Options != null)
                {
                    // Remove existing options
                    _context.Options.RemoveRange(existingQuestion.Options);

                    // Add new options
                    existingQuestion.Options = questionDto.Options.Select(o => new Option
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToArray();
                }

                // Update text answers
                if (questionDto.TextAnswers != null)
                {
                    // Remove existing text answers
                    _context.TextAnswers.RemoveRange(existingQuestion.TextAnswers);

                    // Add new text answers
                    existingQuestion.TextAnswers = questionDto.TextAnswers.Select(ta => new TextAnswer
                    {
                        Text = ta.Text
                    }).ToArray();
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating question: {ex.Message}");
                return false;
            }
        }

        public async Task<QuizApiResponse> BanQuizAsync(Guid quizId, int userId, bool isAdmin)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return QuizApiResponse.Failure("Quiz not found");
            }

            // Only admins can ban quizzes
            if (!isAdmin)
            {
                return QuizApiResponse.Failure("You don't have permission to ban quizzes.");
            }

            quiz.IsBan = true;
            quiz.IsActive = false; // Also set IsActive to false when banned

            await _context.SaveChangesAsync();
            return QuizApiResponse.Success();
        }

        public async Task<QuizApiResponse> UnbanQuizAsync(Guid quizId, int userId, bool isAdmin)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return QuizApiResponse.Failure("Quiz not found");
            }

            // Only admins can unban quizzes
            if (!isAdmin)
            {
                return QuizApiResponse.Failure("You don't have permission to unban quizzes.");
            }

            quiz.IsBan = false;
            // Note: We don't automatically set IsActive back to true when unbanning
            // The quiz creator may want to keep it inactive for other reasons

            await _context.SaveChangesAsync();
            return QuizApiResponse.Success();
        }
    }
}