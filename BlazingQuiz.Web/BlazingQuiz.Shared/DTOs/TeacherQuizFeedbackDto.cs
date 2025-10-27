namespace BlazingQuiz.Shared.DTOs
{
    public class TeacherQuizFeedbackDto
    {
        public List<TeacherQuizRatingDto> Ratings { get; set; } = new List<TeacherQuizRatingDto>();
        public List<TeacherQuizCommentDto> Comments { get; set; } = new List<TeacherQuizCommentDto>();
        public List<TeacherQuizFeedbackItemDto> CombinedFeedback { get; set; } = new List<TeacherQuizFeedbackItemDto>();
    }

    public class TeacherQuizRatingDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Score { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
    }

    public class TeacherQuizCommentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
    }

    public class TeacherQuizFeedbackItemDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string? Score { get; set; }
        public string? Content { get; set; }
        public bool IsCommentDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
    }
}