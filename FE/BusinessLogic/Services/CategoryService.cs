using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryWithCount>> GetAllAsync();
        Task<Category?> GetByIdAsync(short id);
        Task<List<Category>> SearchAsync(string? keyword, bool? isActive);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(short id);
        Task ToggleStatusAsync(short id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IApiClient _apiClient;

        public CategoryService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CategoryWithCount>> GetAllAsync()
        {
            return await _apiClient.GetAsync<List<CategoryWithCount>>("Categories")
                   ?? new List<CategoryWithCount>();
        }

        public async Task<Category?> GetByIdAsync(short id)
        {
            return await _apiClient.GetAsync<Category>($"Categories/{id}");
        }

        public async Task<List<Category>> SearchAsync(string? keyword, bool? isActive)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");

            if (isActive.HasValue)
                queryParams.Add($"isActive={isActive.Value}");

            var query = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(query)
                ? "Categories/Search"
                : $"Categories/Search?{query}";

            return await _apiClient.GetAsync<List<Category>>(endpoint)
                   ?? new List<Category>();
        }

        public async Task CreateAsync(Category category)
        {
            await _apiClient.PostAsync<Category>("Categories", category);
        }

        public async Task UpdateAsync(Category category)
        {
            await _apiClient.PutAsync<object>($"Categories/{category.CategoryId}", category);
        }

        public async Task DeleteAsync(short id)
        {
            await _apiClient.DeleteAsync($"Categories/{id}");
        }

        public async Task ToggleStatusAsync(short id)
        {
            await _apiClient.PutAsync<object>($"Categories/{id}/ToggleStatus", new { });
        }
    }

    // DTO for Category with article count
    public class CategoryWithCount : Category
    {
        public int ArticleCount { get; set; }
    }
}