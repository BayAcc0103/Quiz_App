using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class GoogleLoginDto
    {
        [Required]
        public string GoogleIdToken { get; set; }
        [Required]
        public string Role { get; set; }
    }
}