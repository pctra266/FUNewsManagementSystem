using DataAccess.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace BusinessLogic.Services
{
    public interface IApiService
    {
        Task<LoginResponseModel?> LoginAsync(LoginViewModel loginModel);
        Task<List<T>?> GetAsync<T>(string endpoint);
        Task<T?> GetByIdAsync<T>(string endpoint, object id, string query);
        Task<T?> GetByIdAsync<T>(string endpoint, object id);
        Task<T?> GetByIdAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object id, object data);
        Task<bool> DeleteAsync(string endpoint, object id);
        Task<bool> DeleteAsync(string endpoint);
        Task<string?> UploadImageAsync(string endpoint, Stream fileStream, string fileName);
        Task<byte[]?> DownloadFileAsync(string endpoint);
        Task<bool> RefreshTokenAsync();
        Task<ODataResponse<T>?> GetODataAsync<T>(string endpoint);
    }

    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            
            // _httpClient configuration moved to Program.cs
        }

        private HttpClient GetClient(string endpoint = null)
        {
            string name = DetermineClientName(endpoint);
            var client = _httpClientFactory.CreateClient(name);
            
            // Attach token if exists
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var token = context.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            return client;
        }

        private string DetermineClientName(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return "CoreClient";

            var lowerEndpoint = endpoint.ToLowerInvariant();

            // AI API
            if (lowerEndpoint.Contains("/api/ai/") || lowerEndpoint.Contains("suggest-tags"))
            {
                return "AIClient";
            }

            // Analytics API
            // Covers: /odata/NewsArticles/Default.ArticlesByCategory, ArticlesByAuthor, ArticlesByStatus, Trending
            if (lowerEndpoint.Contains("articlesby") || 
                lowerEndpoint.Contains("trending") || 
                lowerEndpoint.Contains("dashboard") || 
                lowerEndpoint.Contains("export") ||
                lowerEndpoint.Contains("recommend"))
            {
                return "AnalyticsClient";
            }

            // Default to Core API
            return "CoreClient";
        }

        public async Task<LoginResponseModel?> LoginAsync(LoginViewModel loginModel)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginModel, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = GetClient("/api/Auth/login");
                var response = await client.PostAsync("/api/Auth/login", content);
                
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
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (string.IsNullOrWhiteSpace(content)) return new List<T>();

                    // More robust check for OData wrapper
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("value", out var valueProp))
                    {
                        if (valueProp.ValueKind == JsonValueKind.Array)
                        {
                            return JsonSerializer.Deserialize<List<T>>(valueProp.GetRawText(), _jsonOptions);
                        }
                    }
                    
                    return JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);
                }
                
                return null;
            }
            catch
            {
                // Offline Fallback
                if (_memoryCache.TryGetValue($"OFFLINE_CACHE_{endpoint}", out List<T>? cachedData))
                {
                    return cachedData;
                }
                return null;
            }
        }

        public async Task<ODataResponse<T>?> GetODataAsync<T>(string endpoint)
        {
            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content)) return new ODataResponse<T>();

                    return JsonSerializer.Deserialize<ODataResponse<T>>(content, _jsonOptions);
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
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync($"{endpoint}({id})"));
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return DeserializeODataResult<T>(content);
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }
        public async Task<T?> GetByIdAsync<T>(string endpoint, object id, string query)
        {
            try
            {
                var client = GetClient(endpoint);
                var url = $"{endpoint}({id}){query}";
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(url));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return DeserializeODataResult<T>(content);
                }

                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        public async Task<T?> GetByIdAsync<T>(string endpoint)
        {
            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync($"{endpoint}"));
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return DeserializeODataResult<T>(content);
                }
                
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        private T? DeserializeODataResult<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return default(T);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // If OData wraps the result in a "value" property (common for functions)
            if (root.TryGetProperty("value", out var valueProp))
            {
                return JsonSerializer.Deserialize<T>(valueProp.GetRawText(), _jsonOptions);
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, async () => 
                {
                    var innerJson = JsonSerializer.Serialize(data, _jsonOptions);
                    var innerContent = new StringContent(innerJson, Encoding.UTF8, "application/json");
                    return await client.PostAsync(endpoint, innerContent);
                });
                
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
                var url = $"{endpoint}({id})";
                var client = GetClient(endpoint);
                
                var response = await SendRequestWithAuthRetryAsync(client, async () => 
                {
                    var innerJson = JsonSerializer.Serialize(data, _jsonOptions);
                    var innerContent = new StringContent(innerJson, Encoding.UTF8, "application/json");
                    return await client.PutAsync(url, innerContent);
                });
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseContent)) return default(T); // NoContent
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, object id)
        {
            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.DeleteAsync($"{endpoint}({id})"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteAsync(string endpoint)
        {
            var client = GetClient(endpoint);
            var response = await SendRequestWithAuthRetryAsync(client, () => client.DeleteAsync(endpoint));
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> UploadImageAsync(string endpoint, Stream fileStream, string fileName)
        {
            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Adjust based on file type if needed, or let API handle validation
                content.Add(fileContent, "file", fileName);

                var client = GetClient(endpoint);
                var response = await client.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                     // Check if response is JSON object with "url" property
                    try 
                    {
                        var json = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (json.TryGetProperty("url", out var urlProperty))
                        {
                            return urlProperty.GetString();
                        }
                    }
                    catch
                    {
                        // Fallback if plain string
                        return responseContent;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }



        public async Task<byte[]?> DownloadFileAsync(string endpoint)
        {
            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var accessToken = context.Session.GetString("AuthToken");
            var refreshToken = context.Session.GetString("RefreshToken");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return false;

            var request = new TokenRequestModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // We need to call refresh without Authorization header or with it? 
                // Usually refresh endpoint allows anonymous or validation ofexpired token. 
                // Existing client has expired token in header, which is fine.
                
                // Important: Avoid infinite loop if this call returns 401.
                // We use a separate fresh client or ensure no interception for this specific call? 
                // Since our retry logic is manual in methods, we are safe if we don't call retry here.
                
                var client = GetClient("/api/Auth/refresh");
                var response = await client.PostAsync("/api/Auth/refresh", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var newTokens = JsonSerializer.Deserialize<LoginResponseModel>(responseContent, _jsonOptions);

                    if (newTokens != null)
                    {
                        // Update session
                        context.Session.SetString("AuthToken", newTokens.Token);
                        context.Session.SetString("RefreshToken", newTokens.RefreshToken);
                        context.Session.SetString("TokenExpiresAt", newTokens.ExpiresAt.ToString("o"));
                        
                        // Update current client - Not needed as GetClient() reads from session
                        // But need to ensure session is updated
                        
                        return true;
                    }
                }
            }
            catch
            {
                // Log error
            }

            return false;
        }

        private async Task<HttpResponseMessage> SendRequestWithAuthRetryAsync(HttpClient client, Func<Task<HttpResponseMessage>> requestFunc)
        {
            // PROACTIVE REFRESH CHECK
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var expiresAtStr = context.Session.GetString("TokenExpiresAt");
                if (DateTime.TryParse(expiresAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expiresAt))
                {
                    // Check if token expires in less than 5 minutes
                    if (expiresAt < DateTime.UtcNow.AddMinutes(5))
                    {
                        // Try to refresh proactively
                        if (await RefreshTokenAsync())
                        {
                            // Update client with new token
                            var newToken = context.Session.GetString("AuthToken");
                            if (!string.IsNullOrEmpty(newToken))
                            {
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                            }
                        }
                    }
                }
            }

            var response = await requestFunc();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token might be expired, try to refresh
                if (await RefreshTokenAsync())
                {
                    // Update the current client's token from session
                    var context2 = _httpContextAccessor.HttpContext;
                    if (context2 != null)
                    {
                        var token = context2.Session.GetString("AuthToken");
                        if (!string.IsNullOrEmpty(token))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                    }
                    // Retry request
                    return await requestFunc();
                }
            }
            return response;
        }

    }

    public class ODataResponse<T>
    {
        public List<T>? Value { get; set; }
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
    }
}
