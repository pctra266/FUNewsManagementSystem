using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetAllAsync();
        Task<Tag?> GetByIdAsync(int id);
        Task<List<Tag>> SearchAsync(string? keyword);
        Task<List<NewsArticle>> GetArticlesByTagAsync(int tagId);
        Task CreateAsync(Tag tag);
        Task UpdateAsync(Tag tag);
        Task DeleteAsync(int id);
    }

    public class TagService : ITagService
    {
        private readonly IApiClient _apiClient;

        public TagService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<Tag>> GetAllAsync()
        {
            return await _apiClient.GetAsync<List<Tag>>("Tags")
                   ?? new List<Tag>();
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            return await _apiClient.GetAsync<Tag>($"Tags/{id}");
        }

        public async Task<List<Tag>> SearchAsync(string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return await GetAllAsync();

            return await _apiClient.GetAsync<List<Tag>>($"Tags/Search?keyword={Uri.EscapeDataString(keyword)}")
                   ?? new List<Tag>();
        }

        public async Task<List<NewsArticle>> GetArticlesByTagAsync(int tagId)
        {
            return await _apiClient.GetAsync<List<NewsArticle>>($"Tags/{tagId}/Articles")
                   ?? new List<NewsArticle>();
        }

        public async Task CreateAsync(Tag tag)
        {
            await _apiClient.PostAsync<Tag>("Tags", tag);
        }

        public async Task UpdateAsync(Tag tag)
        {
            await _apiClient.PutAsync<object>($"Tags/{tag.TagId}", tag);
        }

        public async Task DeleteAsync(int id)
        {
            await _apiClient.DeleteAsync($"Tags/{id}");
        }
    }
}