using Microsoft.JSInterop;

namespace BlazingQuiz.Web.Services.Theme;

public enum ThemeMode
{
    Light,
    Dark
}

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private ThemeMode _currentTheme;
    public event Action? OnThemeChanged;

    public ThemeMode CurrentTheme => _currentTheme;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _currentTheme = ThemeMode.Light; // default to light mode
    }

    public async Task InitializeThemeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            _currentTheme = string.IsNullOrEmpty(theme) || theme == "light" ? ThemeMode.Light : ThemeMode.Dark;
            await ApplyThemeAsync(_currentTheme);
        }
        catch
        {
            // If localStorage is not available, use default theme
            _currentTheme = ThemeMode.Light;
            await ApplyThemeAsync(_currentTheme);
        }
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        _currentTheme = theme;
        await ApplyThemeAsync(theme);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme.ToString().ToLower());
        OnThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync()
    {
        var newTheme = _currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
        await SetThemeAsync(newTheme);
    }

    private async Task ApplyThemeAsync(ThemeMode theme)
    {
        var themeString = theme == ThemeMode.Dark ? "dark" : "light";
        await _jsRuntime.InvokeVoidAsync("themeUtils.setTheme", themeString);
    }
}