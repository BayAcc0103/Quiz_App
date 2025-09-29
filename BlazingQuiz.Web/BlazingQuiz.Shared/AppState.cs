using BlazingQuiz.Shared;

namespace BlazingQuiz.Shared
{
    public class AppState : IAppState
    {
        public string? LoadingText { get; private set; }

        public event Action? OnToggleLoader;
        public event Action<string>? OnShowError;

        public void HideLoader()
        {
            LoadingText = null;
            OnToggleLoader?.Invoke();
        }

        public void ShowError(string errorText) => 
            OnShowError?.Invoke(errorText);

        public void ShowLoader(string loaderText)
        {
            LoadingText = loaderText;
            OnToggleLoader?.Invoke();
        }
    }
}
