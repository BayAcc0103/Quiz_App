using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class QuizCategory
    {
        [Key]
        public int Id { get; set; }
        
        public Guid QuizId { get; set; }
        public int CategoryId { get; set; }
        
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
        
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }
    }
}