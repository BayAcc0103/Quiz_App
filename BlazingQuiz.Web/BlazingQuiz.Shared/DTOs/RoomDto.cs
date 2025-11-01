using System.ComponentModel.DataAnnotations;

namespace BlazingQuiz.Shared.DTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(6)]
        public string Code { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        public int CreatedBy { get; set; }
        
        public string? CreatedByName { get; set; }
        
        public Guid? QuizId { get; set; } // Selected quiz for the room
        
        public string? QuizName { get; set; } // Name of the selected quiz
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public bool IsActive { get; set; }
        
        public int MaxParticipants { get; set; }
    }
}