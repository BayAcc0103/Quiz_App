using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared
{
    public interface IAppState
    {
        string? LoadingText { get; }
        void ShowLoader(string loaderText);
        void HideLoader();
        event Action? OnToggleLoader;

        event Action<string>? OnShowError;
        void ShowError(string errorText);
        
        event Action<string, AlertType>? OnShowAlert;
        void ShowAlert(string message, AlertType alertType);
    }
}
