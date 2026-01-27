using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface INewsArticleService
    {
        Task<List<NewsArticle>> GetAllAsync();
        Task<List<NewsArticle>> GetPublicAsync();
        Task<NewsArticle?> GetByIdAsync(string id);
        Task<List<NewsArticle>> SearchAsync(string? keyword, short? categoryId, bool? status);
        Task CreateAsync(NewsArticle article);
        Task UpdateAsync(NewsArticle article);
        Task DeleteAsync(string id);
        Task<NewsArticle?> DuplicateAsync(string id);
        Task<List<NewsArticle>> GetRelatedAsync(string id);
    }

    public class NewsArticleService : INewsArticleService
    {
        private readonly IApiClient _apiClient;

        public NewsArticleService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<NewsArticle>> GetAllAsync()
        {
            return await _apiClient.GetAsync<List<NewsArticle>>("NewsArticles")
                   ?? new List<NewsArticle>();
        }

        public async Task<List<NewsArticle>> GetPublicAsync()
        {
            return await _apiClient.GetAsync<List<NewsArticle>>("NewsArticles/public")
                   ?? new List<NewsArticle>();
        }

        public async Task<NewsArticle?> GetByIdAsync(string id)
        {
            return await _apiClient.GetAsync<NewsArticle>($"NewsArticles/{id}");
        }

        public async Task<List<NewsArticle>> SearchAsync(
            string? keyword,
            short? categoryId,
            bool? status)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");

            if (categoryId.HasValue)
                queryParams.Add($"categoryId={categoryId.Value}");

            if (status.HasValue)
                queryParams.Add($"status={status.Value}");

            var query = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(query)
                ? "NewsArticles/Search"
                : $"NewsArticles/Search?{query}";

            return await _apiClient.GetAsync<List<NewsArticle>>(endpoint)
                   ?? new List<NewsArticle>();
        }

        public async Task CreateAsync(NewsArticle article)
        {
            await _apiClient.PostAsync<NewsArticle>("NewsArticles", article);
        }

        public async Task UpdateAsync(NewsArticle article)
        {
            await _apiClient.PutAsync<object>($"NewsArticles/{article.NewsArticleId}", article);
        }

        public async Task DeleteAsync(string id)
        {
            await _apiClient.DeleteAsync($"NewsArticles/{id}");
        }

        public async Task<NewsArticle?> DuplicateAsync(string id)
        {
            return await _apiClient.PostAsync<NewsArticle>($"NewsArticles/{id}/Duplicate", new { });
        }

        public async Task<List<NewsArticle>> GetRelatedAsync(string id)
        {
            return await _apiClient.GetAsync<List<NewsArticle>>($"NewsArticles/{id}/related")
                   ?? new List<NewsArticle>();
        }
    }
}