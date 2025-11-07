namespace BlazingQuiz.Shared.DTOs
{
    public class QuizDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeInMinutes { get; set; }
        public bool IsActive { get; set; }
        public string? Level { get; set; } // Level of the quiz (e.g., Easy, Medium, Hard)
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public List<RatingDto> RecentRatings { get; set; } = new List<RatingDto>();
        public List<CommentDto> RecentComments { get; set; } = new List<CommentDto>();
    }

    public class RatingDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Guid QuizId { get; set; }
        public string Score { get; set; } // Updated to string after previous changes
        public DateTime CreatedOn { get; set; }
        
        public string? StudentName { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Guid QuizId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public string? StudentName { get; set; }
    }
}