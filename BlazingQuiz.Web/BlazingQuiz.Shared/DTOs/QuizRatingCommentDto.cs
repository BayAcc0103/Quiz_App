namespace BlazingQuiz.Shared.DTOs
{
    public class QuizRatingCommentDto
    {
        public int StudentQuizId { get; set; }
        public int RatingScore { get; set; } // Rating from 1 to 5
        public string? CommentContent { get; set; }
    }
}