using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class RoomQuizService
    {
        private readonly QuizContext _context;

        public RoomQuizService(QuizContext context)
        {
            _context = context;
        }

        public async Task<QuizApiResponse<int>> StartQuizAsync(int studentId, Guid quizId, Guid roomId)
        {
            try
            {
                // Check if the student has already started this quiz in this room
                var existingStudentQuizForRoom = await _context.StudentQuizzesForRoom
                    .Where(sqfr => sqfr.StudentId == studentId && sqfr.RoomId == roomId && sqfr.QuizId == quizId)
                    .OrderByDescending(sqfr => sqfr.StartedOn) // Get the most recent one
                    .FirstOrDefaultAsync();

                if (existingStudentQuizForRoom != null)
                {
                    // If the quiz is already started and not completed, return the existing ID
                    if (existingStudentQuizForRoom.Status != nameof(StudentQuizStatus.Completed) &&
                        existingStudentQuizForRoom.Status != nameof(StudentQuizStatus.AutoSubmitted))
                    {
                        return QuizApiResponse<int>.Success(existingStudentQuizForRoom.Id);
                    }
                }

                // Create new StudentQuizForRoom record
                var studentQuizForRoom = new StudentQuizForRoom
                {
                    QuizId = quizId,
                    StudentId = studentId,
                    RoomId = roomId,
                    Status = nameof(StudentQuizStatus.Started),
                    StartedOn = DateTime.UtcNow,
                };
                _context.StudentQuizzesForRoom.Add(studentQuizForRoom);
                await _context.SaveChangesAsync();

                return QuizApiResponse<int>.Success(studentQuizForRoom.Id);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<int>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse<QuestionDto?>> GetNextQuestionForQuizAsync(int studentQuizForRoomId, int studentId)
        {
            //TODO: Try to get the data in less number of database trips
            var studentQuiz = await _context.StudentQuizzesForRoom
                .Include(s => s.StudentQuizQuestionsForRoom)
                .FirstOrDefaultAsync(s => s.Id == studentQuizForRoomId);
            if (studentQuiz == null)
            {
                return QuizApiResponse<QuestionDto?>.Failure("Student quiz for room not found");
            }
            if(studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse<QuestionDto?>.Failure("Invalid request");
            }
            var questionsServed = studentQuiz.StudentQuizQuestionsForRoom
                .Select(s => s.QuestionId)
                .ToArray();
            var nextQuestion = await _context.Questions
                .Include(q => q.TextAnswers)
                .Include(q => q.Options)
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
                    TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToList() : new List<TextAnswerDto>(), // Only include text answers if it's a text answer question
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
                var studentQuizQuestion = new StudentQuizQuestionForRoom
                {
                    QuestionId = nextQuestion.Id,
                    StudentQuizForRoomId = studentQuizForRoomId,
                    OptionId = 0 // Initialize with a default value, will be updated on save
                };
                _context.StudentQuizQuestionsForRoom.Add(studentQuizQuestion);
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
            Console.WriteLine($"SaveQuestionResponseAsync called - StudentQuizForRoomId: {dto.StudentQuizId}, QuestionId: {dto.QuestionId}, OptionId: {dto.OptionId}, TextAnswer: '{dto.TextAnswer}'");

            var studentQuiz = await _context.StudentQuizzesForRoom.AsTracking()
               .FirstOrDefaultAsync(s => s.Id == dto.StudentQuizId);
            if (studentQuiz == null)
            {
                Console.WriteLine("Student quiz for room not found");
                return QuizApiResponse.Failure("Student quiz for room not found");
            }
            if (studentQuiz.StudentId != studentId)
            {
                Console.WriteLine("Invalid request - student ID mismatch");
                return QuizApiResponse.Failure("Invalid request");
            }

            var studentQuizQuestion = await _context.StudentQuizQuestionsForRoom.AsTracking()
                .FirstOrDefaultAsync(sqq => sqq.StudentQuizForRoomId == dto.StudentQuizId && sqq.QuestionId == dto.QuestionId);

            if (studentQuizQuestion == null)
            {
                Console.WriteLine("Student quiz question for room not found");
                return QuizApiResponse.Failure("Student quiz question for room not found.");
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

                // Get all possible correct text answers for this question
                var correctTextAnswers = await _context.TextAnswers
                    .Where(ta => ta.QuestionId == dto.QuestionId)
                    .Select(ta => ta.Text)
                    .ToArrayAsync();

                // Debug logging
                Console.WriteLine($"Found {correctTextAnswers.Length} correct text answers for question {dto.QuestionId}");

                // Compare student's answer with all correct answers (case-insensitive and trimmed)
                // Handle null/empty cases correctly
                if (!string.IsNullOrWhiteSpace(dto.TextAnswer) && correctTextAnswers.Length > 0)
                {
                    var trimmedStudentAnswer = dto.TextAnswer.Trim();

                    bool isCorrect = false;
                    foreach (var correctAnswer in correctTextAnswers)
                    {
                        if (!string.IsNullOrWhiteSpace(correctAnswer))
                        {
                            var trimmedCorrect = correctAnswer.Trim();

                            // Debug logging for troubleshooting
                            Console.WriteLine($"Comparing text answers - Correct: '{trimmedCorrect}' | Student: '{trimmedStudentAnswer}'");

                            if (string.Equals(trimmedStudentAnswer, trimmedCorrect, StringComparison.OrdinalIgnoreCase))
                            {
                                isCorrect = true;
                                break;
                            }
                        }
                    }

                    if (isCorrect)
                    {
                        studentQuiz.Total++; // Mark as correct
                        Console.WriteLine("Text answer matched - incrementing score");
                    }
                    else
                    {
                        Console.WriteLine("Text answer did not match any of the correct answers");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping text answer comparison - Student answer empty/null: {string.IsNullOrWhiteSpace(dto.TextAnswer)} | Correct answers count: {correctTextAnswers.Length}");
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

        public async Task<QuizApiResponse> SubmitQuizAsync(int studentQuizForRoomId, int studentId) =>
            await CompleteQuizAsync(studentQuizForRoomId, DateTime.UtcNow, nameof(StudentQuizStatus.Completed), studentId);

        public async Task<QuizApiResponse> ExitQuizAsync(int studentQuizForRoomId, int studentId) =>
            await CompleteQuizAsync(studentQuizForRoomId, null, nameof(StudentQuizStatus.Exited), studentId);

        public async Task<QuizApiResponse> AutoSubmitQuizAsync(int studentQuizForRoomId, int studentId) =>
            await CompleteQuizAsync(studentQuizForRoomId, DateTime.UtcNow, nameof(StudentQuizStatus.AutoSubmitted), studentId);

        public async Task<QuizApiResponse> CompleteQuizAsync(int studentQuizForRoomId, DateTime? completedOn, string status, int studentId)
        {
            var studentQuiz = await _context.StudentQuizzesForRoom.AsTracking()
                .FirstOrDefaultAsync(s => s.Id == studentQuizForRoomId);
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

        public async Task<QuizApiResponse<QuizResultDto>> GetQuizResultAsync(int studentQuizForRoomId, int studentId)
        {
            var studentQuiz = await _context.StudentQuizzesForRoom
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Options)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.TextAnswers)
                .Include(sq => sq.StudentQuizQuestionsForRoom)
                .FirstOrDefaultAsync(sq => sq.Id == studentQuizForRoomId);

            if (studentQuiz == null)
            {
                return QuizApiResponse<QuizResultDto>.Failure("Student quiz for room not found.");
            }

            if (studentQuiz.StudentId != studentId)
            {
                return QuizApiResponse<QuizResultDto>.Failure("Invalid request.");
            }

            var quizResult = new QuizResultDto
            {
                Id = studentQuiz.Id,
                QuizName = studentQuiz.Quiz.Name,
                QuizImagePath = studentQuiz.Quiz.ImagePath,
                StartedOn = studentQuiz.StartedOn,
                CompletedOn = studentQuiz.CompletedOn,
                TotalQuestions = studentQuiz.Quiz.Questions.Count,
                CorrectAnswers = studentQuiz.Total,
                IncorrectAnswers = studentQuiz.Quiz.Questions.Count - studentQuiz.Total,
                Questions = studentQuiz.Quiz.Questions.Select(q => new QuizResultQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    ImagePath = q.ImagePath,
                    IsTextAnswer = q.IsTextAnswer,
                    Options = q.Options.Select(o => new QuizResultOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text
                    }).ToList(),
                    TextAnswers = q.IsTextAnswer ? q.TextAnswers.Select(ta => new TextAnswerDto
                    {
                        Id = ta.Id,
                        Text = ta.Text
                    }).ToList() : new List<TextAnswerDto>(),
                    SelectedOptionId = studentQuiz.StudentQuizQuestionsForRoom
                        .FirstOrDefault(sqq => sqq.QuestionId == q.Id)?.OptionId ?? 0,
                    SelectedTextAnswer = studentQuiz.StudentQuizQuestionsForRoom
                        .FirstOrDefault(sqq => sqq.QuestionId == q.Id)?.TextAnswer,
                    CorrectOptionId = q.IsTextAnswer ? 0 : q.Options.FirstOrDefault(o => o.IsCorrect)?.Id ?? 0 // For text questions, there's no correct option ID
                }).ToList()
            };

            return QuizApiResponse<QuizResultDto>.Success(quizResult);
        }
    }
}