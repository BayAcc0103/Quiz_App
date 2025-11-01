using BlazingQuiz.Shared.DTOs;

namespace BlazingQuiz.Web.Services
{
    public class ProfileUpdateService
    {
        public event Action<UserDto>? OnProfileUpdated;
        public event Action? OnAvatarUpdated;

        public void NotifyProfileUpdated(UserDto userDto)
        {
            OnProfileUpdated?.Invoke(userDto);
        }

        public void NotifyAvatarUpdated()
        {
            OnAvatarUpdated?.Invoke();
        }
    }
}