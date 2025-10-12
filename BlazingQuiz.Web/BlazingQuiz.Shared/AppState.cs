using BlazingQuiz.Shared;

namespace BlazingQuiz.Shared
{
    public class AppState : IAppState
    {
        public string? LoadingText { get; private set; }

        public event Action? OnToggleLoader;
        public event Action<string>? OnShowError;
        public event Action<string, AlertType>? OnShowAlert;

        public void HideLoader()
        {
            LoadingText = null;
            OnToggleLoader?.Invoke();
        }

        public void ShowError(string errorText) => 
            OnShowError?.Invoke(errorText);

        public void ShowAlert(string message, AlertType alertType) => 
            OnShowAlert?.Invoke(message, alertType);

        public void ShowLoader(string loaderText)
        {
            LoadingText = loaderText;
            OnToggleLoader?.Invoke();
        }
    }
}
