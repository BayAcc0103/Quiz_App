using BlazingQuiz.Shared.DTOs;

namespace BlazingQuiz.Shared
{
    public class QuizState
    {
        public int StudentQuizId { get; private set; }
        public QuizListDto? Quiz { get; private set; }
        public int SelectedCategoryId { get; set; } = 0;
        public void StartQuiz(QuizListDto? quiz, int studentQuizId) =>
            (Quiz, StudentQuizId) = (quiz, studentQuizId);
        public void StopQuiz() =>
            (Quiz, StudentQuizId) = (null, 0);
    }
}
