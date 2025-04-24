using BlazingQuiz.Web;
using BlazingQuiz.Web.Apis;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureRefit(builder.Services);

await builder.Build().RunAsync();

static void ConfigureRefit(IServiceCollection services)
{
    const string ApiBaseUrl = "https://localhost:7048";
    services.AddRefitClient<IAuthApi>()
        .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri(ApiBaseUrl));
}
