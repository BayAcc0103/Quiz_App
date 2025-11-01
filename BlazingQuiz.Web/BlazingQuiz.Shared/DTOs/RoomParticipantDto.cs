namespace BlazingQuiz.Shared.DTOs
{
    public class RoomParticipantDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
    }
}