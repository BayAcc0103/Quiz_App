using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazingQuiz.Shared.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, Length(10, 15)]
        public string Phone { get; set; }

        [Required, MaxLength(250)]
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; } = UserRole.Student;
    }
}
