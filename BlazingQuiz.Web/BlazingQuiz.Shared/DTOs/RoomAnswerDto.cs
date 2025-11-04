using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public class RoomAnswerDto
    {
        public int Id { get; set; }
        
        public Guid RoomId { get; set; }
        
        public int UserId { get; set; }
        
        public string UserName { get; set; } = string.Empty;
        
        public int QuestionId { get; set; }
        
        public int? OptionId { get; set; }
        
        public string? TextAnswer { get; set; }
        
        public DateTime AnsweredAt { get; set; }
    }
}