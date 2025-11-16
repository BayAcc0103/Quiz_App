using BlazingQuiz.Shared;
using Microsoft.JSInterop;

namespace BlazingQuiz.Web.Services
{
    public class SessionStorageService : IStorageService
    {
        private readonly IJSRuntime _jSRuntime;

        public SessionStorageService(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public async ValueTask<string?> GetItem(string key) =>
            await _jSRuntime.InvokeAsync<string>("sessionStorage.getItem", key);

        public async ValueTask RemoveItem(string key) =>
            await _jSRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);

        public async ValueTask SetItem(string key, string value) =>
            await _jSRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
    }
}