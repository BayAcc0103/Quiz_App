using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 50, ErrorMessage = "Maximum players must be between 1 and 50.")]
        public int MaxParticipants { get; set; } = 50;

        public Guid? QuizId { get; set; } // Selected quiz for the room
    }
}