using BlazingQuiz.Shared;

namespace BlazingQuiz.Shared
{
    public class AppState : IAppState
    {
        public string? LoadingText { get; private set; }

        public event Action? OnToggleLoader;
        public void HideLoader()
        {
            LoadingText = null;
            OnToggleLoader?.Invoke();
        }

        public void ShowLoader(string loaderText)
        {
            LoadingText = loaderText;
            OnToggleLoader?.Invoke();
        }
    }
}
