using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class QuizBookmark
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public Guid QuizId { get; set; }
        
        [MaxLength(100)]
        public string QuizName { get; set; }
        
        [MaxLength(100)]
        public string CategoryName { get; set; }
        
        public DateTime BookmarkedOn { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
    }
}