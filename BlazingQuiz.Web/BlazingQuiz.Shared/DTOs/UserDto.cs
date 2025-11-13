using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared.DTOs
{
    //public record UserDto(int Id, string Name, string Email, string Phone, bool IsApproved);

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? AvatarPath { get; set; }
        public string Role { get; set; } = string.Empty;

        public string FullName
        {
            get => Name;
            set => Name = value;
        }

        public UserDto() { } // Parameterless constructor for Blazor

        public UserDto(int id, string name, string email, string phone, bool isApproved, string? avatarPath = null, string role = "")
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            IsApproved = isApproved;
            AvatarPath = avatarPath;
            Role = role;
        }
    }

}
