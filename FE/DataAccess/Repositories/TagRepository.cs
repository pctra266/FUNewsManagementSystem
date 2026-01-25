using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class TagRepository: ITagRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7000/api/Tags"; // ĐỔI PORT
        private readonly JsonSerializerOptions _options;

        public TagRepository()
        {
            _httpClient = new HttpClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ... (Các hàm Get bạn tự viết tương tự Category) ...

        public async Task CreateTagAsync(Tag tag)
        {
            var json = JsonSerializer.Serialize(tag);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiBaseUrl, content);
            response.EnsureSuccessStatusCode();
        }
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            // Gọi API GET: api/Tags
            var response = await _httpClient.GetAsync(_apiBaseUrl);

            // Nếu API lỗi hoặc rỗng thì trả về list rỗng (hoặc throw exception tùy bạn)
            if (!response.IsSuccessStatusCode) return new List<Tag>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tag>>(content, _options) ?? new List<Tag>();
        }



        public async Task UpdateTagAsync(Tag tag)
        {
            var json = JsonSerializer.Serialize(tag);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gọi API PUT: api/Tags/{id}
            // Lưu ý: Property trong Model mới là TagId (chữ d thường)
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{tag.TagId}", content);

            response.EnsureSuccessStatusCode();
        }
        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            // Gọi API GET: api/Tags/{id}
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Tag>(content, _options);
        }

        public async Task DeleteTagAsync(int id)
        {
            // Gọi API DELETE: api/Tags/{id}
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
