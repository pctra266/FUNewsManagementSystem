using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface INewsTagService
    {
        Task<List<Tag>> GetTagsByArticleAsync(string articleId);
        Task AddTagToArticleAsync(string articleId, int tagId);
        Task RemoveTagFromArticleAsync(string articleId, int tagId);
        Task UpdateArticleTagsAsync(string articleId, List<int> tagIds);
    }

    public class NewsTagService : INewsTagService
    {
        private readonly IApiClient _apiClient;

        public NewsTagService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<Tag>> GetTagsByArticleAsync(string articleId)
        {
            return await _apiClient.GetAsync<List<Tag>>($"NewsTag/Article/{articleId}")
                   ?? new List<Tag>();
        }

        public async Task AddTagToArticleAsync(string articleId, int tagId)
        {
            var request = new { ArticleId = articleId, TagId = tagId };
            await _apiClient.PostAsync<object>("NewsTag/AddTag", request);
        }

        public async Task RemoveTagFromArticleAsync(string articleId, int tagId)
        {
            var request = new { ArticleId = articleId, TagId = tagId };
            await _apiClient.DeleteAsync($"NewsTag/RemoveTag");
        }

        public async Task UpdateArticleTagsAsync(string articleId, List<int> tagIds)
        {
            var request = new { ArticleId = articleId, TagIds = tagIds };
            await _apiClient.PutAsync<object>("NewsTag/UpdateArticleTags", request);
        }
    }
}