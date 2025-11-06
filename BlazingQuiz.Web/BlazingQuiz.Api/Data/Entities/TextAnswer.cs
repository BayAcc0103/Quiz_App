using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class TextAnswer
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(500)]
        public string Text { get; set; }
        
        public int QuestionId { get; set; }
        
        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; }
    }
}