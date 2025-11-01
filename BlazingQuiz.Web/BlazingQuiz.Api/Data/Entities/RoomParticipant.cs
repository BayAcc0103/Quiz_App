using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class RoomParticipant
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid RoomId { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}