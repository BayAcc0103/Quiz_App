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
        public List<TextAnswerDto> TextAnswers { get; set; } = new();
        public int SelectedOptionId { get; set; }
        public int CorrectOptionId { get; set; }
        public string? SelectedTextAnswer { get; set; }
        public bool IsTextAnswer { get; set; }
        public string? CorrectTextAnswer => TextAnswers.FirstOrDefault()?.Text;
        public bool IsTextAnswerCorrect { 
            get 
            {
                // If this is not a text answer question, return false
                if (!IsTextAnswer) return false;
                
                // If no answer was provided, return false (not correct)
                if (string.IsNullOrWhiteSpace(SelectedTextAnswer))
                {
                    return false;
                }
                
                // Compare the selected answer with all correct text answers (case-insensitive and trimmed)
                var trimmedSelected = SelectedTextAnswer.Trim();
                return TextAnswers.Any(textAnswer => 
                    string.Equals(trimmedSelected, textAnswer.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                );
            } 
        }
    }

    public class QuizResultOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
