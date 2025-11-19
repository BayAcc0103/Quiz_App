using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        
        [MaxLength(255)]
        public string? ImagePath { get; set; }

        public bool IsDisplay { get; set; } = false;

        public int? CreatedBy { get; set; } // Teacher ID who created the category
    }
}
