using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class NewsRepository: INewsRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7000/api/NewsArticle"; // ĐỔI PORT
        private readonly JsonSerializerOptions _options;

        public NewsRepository()
        {
            _httpClient = new HttpClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<NewsArticle>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            // Xử lý trường hợp API trả về null hoặc lỗi
            if (!response.IsSuccessStatusCode) return new List<NewsArticle>();

            return JsonSerializer.Deserialize<List<NewsArticle>>(content, _options) ?? new List<NewsArticle>();
        }

        public async Task<NewsArticle?> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NewsArticle>(content, _options);
        }

        public async Task CreateAsync(NewsArticle article)
        {
            var json = JsonSerializer.Serialize(article);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(_apiUrl, content);
        }

        public async Task UpdateAsync(NewsArticle article)
        {
            var json = JsonSerializer.Serialize(article);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"{_apiUrl}/{article.NewsArticleId}", content);
        }

        public async Task DeleteAsync(string id)
        {
            await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
        }
        public async Task<List<NewsStatistic>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // Format ngày tháng để gửi lên API (yyyy-MM-dd)
            string startStr = startDate.ToString("yyyy-MM-dd");
            string endStr = endDate.ToString("yyyy-MM-dd");

            var url = $"{_apiUrl}/Statistics?startDate={startStr}&endDate={endStr}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<NewsStatistic>>(url);
                return result ?? new List<NewsStatistic>();
            }
            catch
            {
                // Nếu lỗi hoặc API chưa viết xong thì trả về list rỗng để không crash web
                return new List<NewsStatistic>();
            }
        }
    }
}
