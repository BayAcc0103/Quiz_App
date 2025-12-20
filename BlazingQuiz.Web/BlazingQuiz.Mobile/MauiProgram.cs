using BlazingQuiz.Mobile.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.Components.Apis;
using BlazingQuiz.Shared.Components.Auth;
using BlazingQuiz.Shared.Components.Services;
using BlazingQuiz.Shared.Components.Services.SignalR;
using BlazingQuiz.Web.Apis;
using BlazingQuiz.Shared.Components.Services.Theme;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Refit;


#if ANDROID
using Xamarin.Android.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#elif IOS
using Security;
#endif

namespace BlazingQuiz.Mobile
{
    public static class MauiProgram
    {
        const string ApiBaseUrl = "https://b861mvjb-7048.asse.devtunnels.ms";
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<QuizAuthStateProvider>();
            builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<QuizAuthStateProvider>());
            builder.Services.AddAuthorizationCore();

            builder.Services.AddSingleton<IStorageService, StorageService>()
                .AddSingleton<QuizState>()
                .AddSingleton<IAppState, AppState>()
                .AddSingleton<IPlatform, MobilePlatform>()
                .AddSingleton<ProfileUpdateService>()
                .AddSingleton<QuizHubService>()
                .AddSingleton<ThemeService>(serviceProvider =>
                {
                    var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
                    var platform = serviceProvider.GetRequiredService<IPlatform>();
                    var storageService = serviceProvider.GetRequiredService<IStorageService>();
                    return new ThemeService(jsRuntime, platform, storageService);
                });

            // Register services that depend on HttpClient and QuizAuthStateProvider
            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new QuizImageService(httpClient, authStateProvider);
            });

            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new QuestionImageService(httpClient, authStateProvider);
            });

            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new QuizAudioService(httpClient, authStateProvider);
            });

            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new QuestionAudioService(httpClient, authStateProvider);
            });

            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new UserAvatarService(httpClient, authStateProvider);
            });

            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new CategoryImageService(httpClient, authStateProvider);
            });

            ConfigureRefit(builder.Services);
            return builder.Build();
        }

        static void ConfigureRefit(IServiceCollection services)
        {
            //https://localhost:7048
            var apiBaseUrl = "https://b861mvjb-7048.asse.devtunnels.ms/";
            if(DeviceInfo.DeviceType == DeviceType.Physical || DeviceInfo.Platform == DevicePlatform.iOS)
            {
                apiBaseUrl = "https://b861mvjb-7048.asse.devtunnels.ms/";
            }
            else if(DeviceInfo.Platform == DevicePlatform.Android)
                {
                    apiBaseUrl = "https://10.0.2.2:7048";
                }

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

            services.AddRefitClient<IRoomApi>(GetRefitSettings)
               .ConfigureHttpClient(SetHttpClient);

            services.AddRefitClient<IRoomQuizApi>(GetRefitSettings)
               .ConfigureHttpClient(SetHttpClient);

            services.AddRefitClient<IUserAvatarApi>(GetRefitSettings)
               .ConfigureHttpClient(SetHttpClient);


            void SetHttpClient(HttpClient httpClient) =>
                httpClient.BaseAddress = new Uri(apiBaseUrl);
            static RefitSettings GetRefitSettings(IServiceProvider sp)
            {
                var authStateProvider = sp.GetRequiredService<QuizAuthStateProvider>();
                return new RefitSettings
                {
                    AuthorizationHeaderValueGetter = (_, __) => Task.FromResult(authStateProvider.User?.Token ?? ""),
                    HttpMessageHandlerFactory = () =>
                    {
#if ANDROID
                        var androidMessageHandler = new AndroidClientHandler()
                        {
                            ServerCertificateCustomValidationCallback =
                            (HttpRequestMessage request, X509Certificate2? certificate2, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
                                certificate2?.Issuer == "CN=localhost" || sslPolicyErrors == SslPolicyErrors.None
                        };
                        return androidMessageHandler;
#elif IOS
                        var nsUrlSessionHandler = new NSUrlSessionHandler();
                        nsUrlSessionHandler.TrustOverrideForUrl = (NSUrlSessionHandler sender, string url, SecTrust trust) =>
                            url.StartsWith("https://localhost");
                            return nsUrlSessionHandler;
#endif
                        return null;
                    }
                };
            }
        }
    }
}
