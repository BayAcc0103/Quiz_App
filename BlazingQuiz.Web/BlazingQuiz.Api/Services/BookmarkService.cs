using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class BookmarkService
    {
        private readonly QuizContext _context;

        public BookmarkService(QuizContext context)
        {
            _context = context;
        }

        public async Task<QuizApiResponse<bool>> AddBookmarkAsync(QuizBookmarkDto bookmarkDto)
        {
            try
            {
                // Check if the quiz exists
                var quiz = await _context.Quizzes.FindAsync(bookmarkDto.QuizId);
                if (quiz == null)
                {
                    return QuizApiResponse<bool>.Failure("Quiz not found");
                }

                // Check if bookmark already exists
                var existingBookmark = await _context.QuizBookmarks
                    .FirstOrDefaultAsync(b => b.UserId == bookmarkDto.UserId && b.QuizId == bookmarkDto.QuizId);

                if (existingBookmark != null)
                {
                    return QuizApiResponse<bool>.Failure("Quiz is already bookmarked");
                }

                var bookmark = new QuizBookmark
                {
                    UserId = bookmarkDto.UserId,
                    QuizId = bookmarkDto.QuizId,
                    QuizName = bookmarkDto.QuizName,
                    CategoryName = bookmarkDto.CategoryName,
                    BookmarkedOn = DateTime.UtcNow
                };

                _context.QuizBookmarks.Add(bookmark);
                await _context.SaveChangesAsync();

                return QuizApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<bool>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse<bool>> RemoveBookmarkAsync(int userId, Guid quizId)
        {
            try
            {
                var bookmark = await _context.QuizBookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.QuizId == quizId);

                if (bookmark == null)
                {
                    return QuizApiResponse<bool>.Failure("Bookmark not found");
                }

                _context.QuizBookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();

                return QuizApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<bool>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse<bool>> IsBookmarkedAsync(int userId, Guid quizId)
        {
            try
            {
                var bookmark = await _context.QuizBookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.QuizId == quizId);

                return QuizApiResponse<bool>.Success(bookmark != null);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<bool>.Failure(ex.Message);
            }
        }

        public async Task<QuizApiResponse<List<QuizBookmarkDto>>> GetBookmarksAsync(int userId)
        {
            try
            {
                var bookmarks = await _context.QuizBookmarks
                    .Where(b => b.UserId == userId)
                    .Select(b => new QuizBookmarkDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        QuizId = b.QuizId,
                        QuizName = b.QuizName,
                        CategoryName = b.CategoryName,
                        BookmarkedOn = b.BookmarkedOn
                    })
                    .ToListAsync();

                return QuizApiResponse<List<QuizBookmarkDto>>.Success(bookmarks);
            }
            catch (Exception ex)
            {
                return QuizApiResponse<List<QuizBookmarkDto>>.Failure(ex.Message);
            }
        }
    }
}