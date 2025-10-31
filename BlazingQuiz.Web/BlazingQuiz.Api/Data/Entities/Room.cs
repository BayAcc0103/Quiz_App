using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; }
        
        [MaxLength(6)]
        [Required]
        public string Code { get; set; } // 6-digit room code
        
        [MaxLength(100)]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        public int CreatedBy { get; set; } // Teacher ID who created the room
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? EndedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int MaxParticipants { get; set; } = 50; // Default max participants
        
        public virtual User? CreatedByUser { get; set; }
        
        public virtual ICollection<RoomQuiz> RoomQuizzes { get; set; } = [];
    }
}