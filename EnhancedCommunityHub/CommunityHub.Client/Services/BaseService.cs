using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CommunityHub.Client.Services
{
    public class BaseService
    {
        protected readonly HttpClient _httpClient;
        protected BaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        protected async Task PostAsync<T>(string url, T data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            response.EnsureSuccessStatusCode();
        }
    }
}
