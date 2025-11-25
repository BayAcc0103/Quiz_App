using BlazingQuiz.Shared;
using BlazingQuiz.Shared.Components;
using BlazingQuiz.Web;
using BlazingQuiz.Web.Apis;
using BlazingQuiz.Web.Auth;
using BlazingQuiz.Web.Services;
using BlazingQuiz.Web.Services.SignalR;
using BlazingQuiz.Web.Services.Theme;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

const string ApiBaseUrl = "https://b861mvjb-7048.asse.devtunnels.ms";

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<QuizAuthStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<QuizAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IAppState, AppState>()
    .AddSingleton<QuizState>()
    .AddSingleton<IStorageService, SessionStorageService>()
    .AddSingleton<IPlatform, WebPlatform>()
    .AddScoped<ExampleJsInterop>()
    .AddSingleton<ProfileUpdateService>()
    .AddSingleton<QuizHubService>()
    .AddScoped<ThemeService>();

builder.Services.AddHttpClient<CategoryImageService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddHttpClient<QuizImageService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddHttpClient<QuestionImageService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddHttpClient<QuizAudioService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddHttpClient<QuestionAudioService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

builder.Services.AddHttpClient<UserAvatarService>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
});

ConfigureRefit(builder.Services);

await builder.Build().RunAsync();

static void ConfigureRefit(IServiceCollection services)
{
    services.AddRefitClient<IAuthApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<ICategoryApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IQuizApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IAdminApi>(GetRefitSettings)
    .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IStudentQuizApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IBookmarkApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IUserAvatarApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IRoomApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IRoomQuizApi>(GetRefitSettings)
        .ConfigureHttpClient(SetHttpClient);

    // Public API without authentication
    services.AddRefitClient<IPublicQuizApi>()
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IPublicCategoryApi>()
        .ConfigureHttpClient(SetHttpClient);

    // Register media services
    services.AddScoped(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new QuizImageService(httpClient, authStateProvider);
    });

    services.AddScoped(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new QuestionImageService(httpClient, authStateProvider);
    });

    services.AddScoped(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new QuizAudioService(httpClient, authStateProvider);
    });

    services.AddScoped(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new QuestionAudioService(httpClient, authStateProvider);
    });

    services.AddScoped(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new UserAvatarService(httpClient, authStateProvider);
    });

    static void SetHttpClient(HttpClient httpClient) =>
        httpClient.BaseAddress = new Uri(ApiBaseUrl);
    static RefitSettings GetRefitSettings(IServiceProvider sp)
    {
        var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
        return new RefitSettings
        {
            AuthorizationHeaderValueGetter = (_, __) => Task.FromResult(authStateProvider.User?.Token ?? "")
        };
    }
}
