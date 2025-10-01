using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization; // Add this using directive
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    public class LoginDto
    {
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; } = UserRole.Student;
    }
}
