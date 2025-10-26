namespace BlazingQuiz.Shared.DTOs
{
    public class QuizAllFeedbackDto
    {
        public List<RatingDto> Ratings { get; set; } = new List<RatingDto>();
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<CombinedFeedbackDto> CombinedFeedback { get; set; } = new List<CombinedFeedbackDto>();
    }

    public class CombinedFeedbackDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string? Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}