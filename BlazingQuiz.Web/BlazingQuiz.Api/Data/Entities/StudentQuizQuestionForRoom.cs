using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuiz.Api.Data.Entities
{
    public class StudentQuizQuestionForRoom
    {
        [Key]
        public int StudentQuizForRoomId { get; set; }
        
        [Key]
        public int QuestionId { get; set; }
        
        public int OptionId { get; set; }
        public string? TextAnswer { get; set; }

        public virtual StudentQuizForRoom StudentQuizForRoom { get; set; }

        public virtual Question Question { get; set; }
    }
}