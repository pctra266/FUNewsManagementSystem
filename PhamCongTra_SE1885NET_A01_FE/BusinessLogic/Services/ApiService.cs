using DataAccess.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services
{
    public interface IApiService
    {
        Task<LoginResponseModel?> LoginAsync(LoginViewModel loginModel);
        Task<List<T>?> GetAsync<T>(string endpoint);
        Task<T?> GetByIdAsync<T>(string endpoint, object id);
        Task<T?> GetByIdAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object id, object data);
        Task<bool> DeleteAsync(string endpoint, object id);
        void SetAuthToken(string token);
        void ClearAuthToken();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Set base address from configuration
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7215";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LoginResponseModel?> LoginAsync(LoginViewModel loginModel)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginModel, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LoginResponseModel>(responseContent, _jsonOptions);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<T>?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Handle OData response format
                    if (content.Contains("\"value\":"))
                    {
                        var odataResponse = JsonSerializer.Deserialize<ODataResponse<T>>(content, _jsonOptions);
                        return odataResponse?.Value;
                    }
                    else
                    {
                        return JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<T?> GetByIdAsync<T>(string endpoint, object id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{endpoint}({id})");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }
        
        public async Task<T?> GetByIdAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object id, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{endpoint}({id})";
                Console.WriteLine($"============== API PUT REQUEST ==============");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Data: {json}");
                Console.WriteLine($"Auth Header: {_httpClient.DefaultRequestHeaders.Authorization?.ToString() ?? "NULL"}");

                var response = await _httpClient.PutAsync(url, content);
                
                Console.WriteLine($"Response Status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");
                Console.WriteLine($"=============================================");
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                else
                {
                    Console.WriteLine($"? PUT request failed with status {response.StatusCode}");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? PUT request exception: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, object id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{endpoint}({id})");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private class ODataResponse<T>
        {
            public List<T>? Value { get; set; }
            public int Count { get; set; }
        }
    }
}