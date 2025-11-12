using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazingQuiz.Api.Data.Entities;

namespace BlazingQuiz.Api.Data.Entities
{
    public class StudentQuizForRoom
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        
        public Guid QuizId { get; set; }
        
        public Guid RoomId { get; set; }
        
        public DateTime StartedOn { get; set; }
        
        public DateTime? CompletedOn { get; set; }

        [AllowedValues(
            nameof(StudentQuizStatus.Started),
            nameof(StudentQuizStatus.Completed),
            nameof(StudentQuizStatus.Exited),
            nameof(StudentQuizStatus.AutoSubmitted)
            )]
        public string Status { get; set; } = nameof(StudentQuizStatus.Started);
        
        public int Total { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual User Student { get; set; }

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
        
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }
        
        public virtual ICollection<StudentQuizQuestion> StudentQuizQuestions { get; set; } = [];
        
        public virtual ICollection<StudentQuizQuestionForRoom> StudentQuizQuestionsForRoom { get; set; } = [];
    }
}