using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7000/api/Category"; // ĐỔI PORT
        private readonly JsonSerializerOptions _options;

        public CategoryRepository()
        {
            _httpClient = new HttpClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<Category>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Category>>(content, _options) ?? new List<Category>();
        }

        public async Task<Category?> GetByIdAsync(short id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Category>(content, _options);
        }

        public async Task CreateAsync(Category category)
        {
            var json = JsonSerializer.Serialize(category);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(_apiUrl, content);
        }

        public async Task UpdateAsync(Category category)
        {
            var json = JsonSerializer.Serialize(category);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"{_apiUrl}/{category.CategoryId}", content);
        }

        public async Task DeleteAsync(short id)
        {
            await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
        }
    }
}
