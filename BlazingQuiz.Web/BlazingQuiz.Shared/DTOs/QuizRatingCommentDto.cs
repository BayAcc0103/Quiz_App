namespace BlazingQuiz.Shared.DTOs
{
    public class QuizRatingCommentDto
    {
        public int StudentQuizId { get; set; }
        public int? RatingScore { get; set; } // Rating from 1 to 5 (for backward compatibility)
        public string? RatingText { get; set; } // Text representation of the rating (bad, very bad, normal, good, very good)
        public string? CommentContent { get; set; }
    }
}