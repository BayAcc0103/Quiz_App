namespace BlazingQuiz.Shared.DTOs
{
    public class TeacherQuizStudentListDto
    {
        public string QuizName { get; set; }
        public string CategoryName { get; set; }
        public PageResult<TeacherQuizStudentDto> Students { get; set; }
    }
    
    public class TeacherQuizStudentDto
    {
        public string Name { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string Status { get; set; }
        public int Total { get; set; }
    }

    public class TeacherQuizStudentForRoomDto
    {
        public string Name { get; set; }
        public string RoomName { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string Status { get; set; }
        public int Total { get; set; }
    }

    public class TeacherQuizStudentForRoomListDto
    {
        public string QuizName { get; set; }
        public string CategoryName { get; set; }
        public PageResult<TeacherQuizStudentForRoomDto> Students { get; set; }
    }
}