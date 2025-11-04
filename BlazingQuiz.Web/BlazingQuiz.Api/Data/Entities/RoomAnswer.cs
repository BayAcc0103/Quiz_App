using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class RoomAnswer
    {
        [Key]
        public int Id { get; set; }
        
        public Guid RoomId { get; set; } // Reference to the room
        
        public int UserId { get; set; } // Participant who answered
        
        public int QuestionId { get; set; } // Question being answered
        
        public int? OptionId { get; set; } // Selected option ID (for multiple choice)
        
        public string? TextAnswer { get; set; } // Text answer (for open-ended questions)
        
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        
        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; }
    }
}