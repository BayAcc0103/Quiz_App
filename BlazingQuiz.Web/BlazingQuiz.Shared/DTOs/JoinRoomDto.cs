using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class JoinRoomDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Room code must be exactly 6 characters.")]
        public string Code { get; set; }
    }
}