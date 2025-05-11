using BlazingQuiz.Mobile.Services;
using BlazingQuiz.Shared;
using BlazingQuiz.Shared.Components.Auth;
using BlazingQuiz.Web.Apis;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
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
                .AddSingleton<IPlatform, MobilePlatform>();

            ConfigureRefit(builder.Services);
            return builder.Build();
        }
        private static readonly string ApiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                                                    ? "https://10.0.2.2:7048"
                                                    : "https://localhost:7048";
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

            static void SetHttpClient(HttpClient httpClient) =>
                httpClient.BaseAddress = new Uri(ApiBaseUrl);
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
