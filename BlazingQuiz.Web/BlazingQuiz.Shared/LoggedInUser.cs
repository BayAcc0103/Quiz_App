using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared
{
    public record LoggedInUser(int Id, string Name, string Email, string Role, string Token, string? AvatarPath = null)
    {
        public string FullName { get; set; } = string.Empty; // For backward compatibility, FullName can be set and by default is the same as Name
        public string ToJson()=> JsonSerializer.Serialize(this);
        public Claim[] ToClaims() =>
            [
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim(ClaimTypes.Name, Name),
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimTypes.Role, Role),
                new Claim(nameof(Token), Token),

            ];
        public static LoggedInUser? LoadFrom(string json) =>
            !string.IsNullOrWhiteSpace(json)?
            JsonSerializer.Deserialize<LoggedInUser>(json)
            : null;
    };
}
