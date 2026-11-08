using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace RaceDay.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

       
        private const string BaseUrl = "https://localhost:7101/swagger/index.html";

        public ApiService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        private void SetToken()
        {
            // Look for the token stored in the session
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // GET Request
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            SetToken();
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            return default;
        }

        // POST Request
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            SetToken();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            return default;
        }

        // PUT Request
        public async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            SetToken();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        // DELETE Request
        public async Task<bool> DeleteAsync(string endpoint)
        {
            SetToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}