namespace BlazingQuiz.Shared.DTOs
{
    public class AdminQuizStudentListDto
    {
        public string QuizName { get; set; }
        public string CategoryName { get; set; }
        public PageResult<AdminQuizStudentDto> Students { get; set; }
    }
    public class AdminQuizStudentDto
    {
        public string Name { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string Status { get; set; } 
        public int Total { get; set; }
    }
}
