namespace BlazingQuiz.Shared.DTOs
{
    public class QuizResultDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public List<QuizResultQuestionDto> Questions { get; set; } = new();
    }

    public class QuizResultQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<QuizResultOptionDto> Options { get; set; } = new();
        public int SelectedOptionId { get; set; }
        public int CorrectOptionId { get; set; }
    }

    public class QuizResultOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
