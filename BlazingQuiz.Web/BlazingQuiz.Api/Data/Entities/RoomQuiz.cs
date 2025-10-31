using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class RoomQuiz
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid RoomId { get; set; }
        
        public Guid QuizId { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }
        
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
    }
}