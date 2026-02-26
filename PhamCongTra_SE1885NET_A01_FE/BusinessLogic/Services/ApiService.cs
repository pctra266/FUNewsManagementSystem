using DataAccess.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;
using System.Net;

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
        private readonly ICacheKeyRegistry _cacheKeyRegistry;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ICacheKeyRegistry cacheKeyRegistry)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _cacheKeyRegistry = cacheKeyRegistry;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private HttpClient GetClient(string endpoint = null)
        {
            string name = DetermineClientName(endpoint);
            var client = _httpClientFactory.CreateClient(name);

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

            if (lowerEndpoint.Contains("/api/ai/") || lowerEndpoint.Contains("suggest-tags"))
            {
                return "AIClient";
            }

            if (lowerEndpoint.Contains("articlesby") ||
                lowerEndpoint.Contains("trending") ||
                lowerEndpoint.Contains("dashboard") ||
                lowerEndpoint.Contains("export") ||
                lowerEndpoint.Contains("recommend"))
            {
                return "AnalyticsClient";
            }

            return "CoreClient";
        }

        public async Task<LoginResponseModel?> LoginAsync(LoginViewModel loginModel)
        {
            var json = JsonSerializer.Serialize(loginModel, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = GetClient("/api/Auth/login");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("/api/Auth/login", content);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException("Unable to reach the authentication API.", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponseModel>(responseContent, _jsonOptions);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Authentication API failed with status {(int)response.StatusCode} ({response.StatusCode}). Body: {errorContent}");
        }

        public async Task<List<T>?> GetAsync<T>(string endpoint)
        {
            var cacheKey = $"OFFLINE_CACHE_{endpoint}";
            if (_memoryCache.TryGetValue(cacheKey, out List<T>? cachedData))
            {
                return cachedData;
            }

            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(content)) return new List<T>();

                    List<T>? result = null;
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("value", out var valueProp))
                    {
                        if (valueProp.ValueKind == JsonValueKind.Array)
                        {
                            result = JsonSerializer.Deserialize<List<T>>(valueProp.GetRawText(), _jsonOptions);
                        }
                    }

                    result ??= JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);

                    if (result != null)
                    {
                        _memoryCache.Set(cacheKey, result, TimeSpan.FromHours(1));
                        TrackCacheKey(endpoint, cacheKey);
                    }

                    return result;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ODataResponse<T>?> GetODataAsync<T>(string endpoint)
        {
            var cacheKey = $"OFFLINE_CACHE_{endpoint}";
            if (_memoryCache.TryGetValue(cacheKey, out ODataResponse<T>? cachedData))
            {
                return cachedData;
            }

            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content)) return new ODataResponse<T>();

                    var result = JsonSerializer.Deserialize<ODataResponse<T>>(content, _jsonOptions);

                    if (result != null)
                    {
                        _memoryCache.Set(cacheKey, result, TimeSpan.FromHours(1));
                        TrackCacheKey(endpoint, cacheKey);
                    }

                    return result;
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
            var url = $"{endpoint}({id})";
            var cacheKey = $"OFFLINE_CACHE_{url}";
            if (_memoryCache.TryGetValue(cacheKey, out T? cachedData)) return cachedData;

            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(url));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = DeserializeODataResult<T>(content);
                    if (result != null)
                    {
                        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                        TrackCacheKey(url, cacheKey);
                    }
                    return result;
                }

                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> GetByIdAsync<T>(string endpoint, object id, string query)
        {
            var url = $"{endpoint}({id}){query}";
            var cacheKey = $"OFFLINE_CACHE_{url}";
            if (_memoryCache.TryGetValue(cacheKey, out T? cachedData)) return cachedData;

            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(url));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = DeserializeODataResult<T>(content);
                    if (result != null)
                    {
                        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                        TrackCacheKey(url, cacheKey);
                    }
                    return result;
                }

                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> GetByIdAsync<T>(string endpoint)
        {
            var cacheKey = $"OFFLINE_CACHE_{endpoint}";
            if (_memoryCache.TryGetValue(cacheKey, out T? cachedData)) return cachedData;

            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.GetAsync(endpoint));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = DeserializeODataResult<T>(content);
                    if (result != null)
                    {
                        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                        TrackCacheKey(endpoint, cacheKey);
                    }
                    return result;
                }

                return default;
            }
            catch
            {
                return default;
            }
        }

        private T? DeserializeODataResult<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return default;

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

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
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, async () =>
                {
                    var innerJson = JsonSerializer.Serialize(data, _jsonOptions);
                    var innerContent = new StringContent(innerJson, Encoding.UTF8, "application/json");
                    return await client.PostAsync(endpoint, innerContent);
                });

                if (response.IsSuccessStatusCode)
                {
                    ClearCacheByEndpoint(endpoint);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }

                return default;
            }
            catch
            {
                return default;
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
                    ClearCacheByEndpoint(endpoint, id);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseContent)) return default;
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, object id)
        {
            try
            {
                var client = GetClient(endpoint);
                var response = await SendRequestWithAuthRetryAsync(client, () => client.DeleteAsync($"{endpoint}({id})"));
                if (response.IsSuccessStatusCode)
                {
                    ClearCacheByEndpoint(endpoint, id);
                    return true;
                }
                return false;
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
            if (response.IsSuccessStatusCode)
            {
                ClearCacheByEndpoint(endpoint);
                return true;
            }
            return false;
        }

        private void ClearCacheByEndpoint(string endpoint, object? id = null)
        {
            ClearCacheGroup(endpoint);

            if (id != null)
            {
                ClearCacheGroup($"{endpoint}({id})");
            }

            foreach (var related in GetRelatedEndpoints(endpoint))
            {
                ClearCacheGroup(related);
            }
        }

        private void ClearCacheGroup(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return;
            }

            var trackedKeys = _cacheKeyRegistry.ExtractKeys(endpoint);
            foreach (var cacheKey in trackedKeys)
            {
                _memoryCache.Remove(cacheKey);
            }

            _memoryCache.Remove($"OFFLINE_CACHE_{endpoint}");
        }

        private IEnumerable<string> GetRelatedEndpoints(string endpoint)
        {
            if (endpoint.Contains("NewsArticles", StringComparison.OrdinalIgnoreCase))
            {
                yield return "/odata/NewsArticles";
                yield return "/odata/NewsArticles/Default.GetActive()";
                yield return "/odata/NewsArticles/Trending(top=4)";
                yield return "/odata/NewsArticles/Trending(top=5)";
                yield return "/odata/NewsArticles/Trending(top=10)";
                //yield return "/odata/Reports/Default.Trending(top=5)";
                //yield return "/odata/Reports/Default.Dashboard()";
            }

            if (endpoint.Contains("Categories", StringComparison.OrdinalIgnoreCase))
            {
                yield return "/odata/Categories";
                yield return "/odata/CategoriesFunctions/Active";
            }

            if (endpoint.Contains("Tags", StringComparison.OrdinalIgnoreCase))
            {
                yield return "/odata/Tags";
                yield return "/odata/TagsFunctions/Search";
            }
        }

        private void TrackCacheKey(string endpoint, string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            _cacheKeyRegistry.Track(endpoint, cacheKey);
        }

        public async Task<string?> UploadImageAsync(string endpoint, Stream fileStream, string fileName)
        {
            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", fileName);

                var client = GetClient(endpoint);
                var response = await client.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
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

                var client = GetClient("/api/Auth/refresh");
                var response = await client.PostAsync("/api/Auth/refresh", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var newTokens = JsonSerializer.Deserialize<LoginResponseModel>(responseContent, _jsonOptions);

                    if (newTokens != null)
                    {
                        context.Session.SetString("AuthToken", newTokens.Token);
                        context.Session.SetString("RefreshToken", newTokens.RefreshToken);
                        context.Session.SetString("TokenExpiresAt", newTokens.ExpiresAt.ToString("o"));
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private async Task<HttpResponseMessage> SendRequestWithAuthRetryAsync(HttpClient client, Func<Task<HttpResponseMessage>> requestFunc)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var expiresAtStr = context.Session.GetString("TokenExpiresAt");
                if (DateTime.TryParse(expiresAtStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expiresAt))
                {
                    if (expiresAt < DateTime.UtcNow.AddMinutes(5))
                    {
                        Console.WriteLine("---------------------------");
                        Console.WriteLine("-----------Go to here----------------");
                        Console.WriteLine("---------------------------");
                        if (await RefreshTokenAsync())
                        {
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
                if (await RefreshTokenAsync())
                {
                    var context2 = _httpContextAccessor.HttpContext;
                    if (context2 != null)
                    {
                        var token = context2.Session.GetString("AuthToken");
                        if (!string.IsNullOrEmpty(token))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        }
                    }
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
