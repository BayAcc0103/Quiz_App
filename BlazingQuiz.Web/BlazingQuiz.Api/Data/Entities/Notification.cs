using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(500)]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;

        public NotificationType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? QuizId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}