using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        
        [MaxLength(255)]
        public string? ImagePath { get; set; }

        public bool IsDisplay { get; set; } = false;

        public int? CreatedBy { get; set; } // Teacher ID who created the category (nullable for existing categories)

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
