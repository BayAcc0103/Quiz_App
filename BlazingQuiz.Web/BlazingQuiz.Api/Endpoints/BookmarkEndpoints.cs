using BlazingQuiz.Api.Services;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazingQuiz.Api.Endpoints
{
    public static class BookmarkEndpoints
    {
        public static void MapBookmarkEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/bookmark");

            group.MapPost("/add", async (BookmarkService bookmarkService, QuizBookmarkDto bookmark) =>
            {
                return await bookmarkService.AddBookmarkAsync(bookmark);
            });

            group.MapDelete("/remove/{userId:int}/{quizId:guid}", async (BookmarkService bookmarkService, int userId, Guid quizId) =>
            {
                return await bookmarkService.RemoveBookmarkAsync(userId, quizId);
            });

            group.MapGet("/is-bookmarked/{userId:int}/{quizId:guid}", async (BookmarkService bookmarkService, int userId, Guid quizId) =>
            {
                return await bookmarkService.IsBookmarkedAsync(userId, quizId);
            });

            group.MapGet("/get-bookmarks/{userId:int}", async (BookmarkService bookmarkService, int userId) =>
            {
                return await bookmarkService.GetBookmarksAsync(userId);
            });
        }
    }
}