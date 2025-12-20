using Microsoft.JSInterop;
using BlazingQuiz.Shared;

namespace BlazingQuiz.Shared.Components.Services.Theme;

public enum ThemeMode
{
    Light,
    Dark
}

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IPlatform _platform;
    private readonly IStorageService? _storageService;
    private ThemeMode _currentTheme;
    public event Action? OnThemeChanged;

    public ThemeMode CurrentTheme => _currentTheme;

    public ThemeService(IJSRuntime jsRuntime, IPlatform platform, IStorageService? storageService = null)
    {
        _jsRuntime = jsRuntime;
        _platform = platform;
        _storageService = storageService;
        _currentTheme = ThemeMode.Light; // default to light mode
    }

    public async Task InitializeThemeAsync()
    {
        try
        {
            string theme;
            if (_platform.IsWeb)
            {
                // Web implementation
                theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            }
            else
            {
                // Mobile implementation - use local storage service
                if (_storageService == null)
                {
                    // Fallback if storage service not provided
                    _currentTheme = ThemeMode.Light;
                    await ApplyThemeAsync(_currentTheme);
                    return;
                }
                theme = await _storageService.GetItem("theme");
            }

            _currentTheme = string.IsNullOrEmpty(theme) || theme == "light" ? ThemeMode.Light : ThemeMode.Dark;
            await ApplyThemeAsync(_currentTheme);
        }
        catch
        {
            // If storage is not available, use default theme
            _currentTheme = ThemeMode.Light;
            await ApplyThemeAsync(_currentTheme);
        }
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        _currentTheme = theme;
        await ApplyThemeAsync(theme);

        if (_platform.IsWeb)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme.ToString().ToLower());
        }
        else
        {
            if (_storageService != null)
            {
                await _storageService.SetItem("theme", theme.ToString().ToLower());
            }
        }

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
        // Use the same themeUtils function for both web and mobile
        await _jsRuntime.InvokeVoidAsync("themeUtils.setTheme", themeString);
    }
}