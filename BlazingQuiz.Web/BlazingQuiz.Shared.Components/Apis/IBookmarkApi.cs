using BlazingQuiz.Shared.DTOs;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.Components.Apis
{
    public interface IBookmarkApi
    {
        [Post("/api/bookmark/add")]
        Task<QuizApiResponse<bool>> AddBookmarkAsync(QuizBookmarkDto bookmark);

        [Delete("/api/bookmark/remove/{userId}/{quizId}")]
        Task<QuizApiResponse<bool>> RemoveBookmarkAsync(int userId, Guid quizId);

        [Get("/api/bookmark/is-bookmarked/{userId}/{quizId}")]
        Task<QuizApiResponse<bool>> IsBookmarkedAsync(int userId, Guid quizId);

        [Get("/api/bookmark/get-bookmarks/{userId}")]
        Task<QuizApiResponse<List<QuizBookmarkDto>>> GetBookmarksAsync(int userId);
    }
}