using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        [Required, MaxLength(500)]
        public string Text { get; set; }
        public List<OptionDto> Options { get; set; } = [];
    }
}
