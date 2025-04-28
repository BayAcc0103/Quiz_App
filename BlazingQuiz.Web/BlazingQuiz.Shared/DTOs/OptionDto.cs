using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class OptionDto
    {
        public int Id { get; set; }
        [Required, MaxLength(500)]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
