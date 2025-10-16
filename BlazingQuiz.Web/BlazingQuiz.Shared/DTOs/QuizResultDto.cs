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
        public string? SelectedTextAnswer { get; set; }
        public string? CorrectTextAnswer { get; set; }
        public bool IsTextAnswer { get; set; }
        public bool IsTextAnswerCorrect { 
            get 
            {
                // If this is not a text answer question, return false
                if (!IsTextAnswer) return false;
                
                // If either answer is null/empty, check if both are null/empty
                if (string.IsNullOrWhiteSpace(SelectedTextAnswer) || string.IsNullOrWhiteSpace(CorrectTextAnswer))
                {
                    return string.IsNullOrWhiteSpace(SelectedTextAnswer) && string.IsNullOrWhiteSpace(CorrectTextAnswer);
                }
                
                // Compare the text answers (case-insensitive and trimmed)
                return string.Equals(SelectedTextAnswer.Trim(), CorrectTextAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            } 
        }
    }

    public class QuizResultOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
