using DataAccess.Models;

namespace BussinessLogic.Services
{
    public interface INewsArticleService
    {
        Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync();
        Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
        Task<IEnumerable<NewsArticle>> GetActiveNewsArticlesAsync();
        Task<IEnumerable<NewsArticle>> GetNewsArticlesByAuthorAsync(short authorId);
        Task<IEnumerable<NewsArticle>> GetNewsArticlesByCategoryAsync(short categoryId);
        Task<NewsArticle> CreateNewsArticleAsync(NewsArticle article, IEnumerable<int>? tagIds = null);
        Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, IEnumerable<int>? tagIds = null);
        Task<bool> DeleteNewsArticleAsync(string id, short? userId = null);
        Task<IEnumerable<NewsArticle>> SearchNewsArticlesAsync(
            string? title = null, 
            string? authorName = null, 
            string? categoryName = null, 
            bool? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<NewsArticle> DuplicateArticleAsync(string originalId, short newAuthorId);
        Task<IEnumerable<NewsArticle>> GetRelatedNewsAsync(string articleId, int limit = 3);
        string GenerateNewsArticleId();
        IQueryable<NewsArticle> GetNewsArticlesQueryable();
        
        // New methods for summary data (without full content)
        Task<IEnumerable<NewsArticle>> GetActiveNewsArticlesSummaryAsync();
        Task<IEnumerable<NewsArticle>> GetNewsArticlesByAuthorSummaryAsync(short authorId);
        Task<IEnumerable<NewsArticle>> GetNewsArticlesByCategorySummaryAsync(short categoryId);
        Task<IEnumerable<NewsArticle>> GetRelatedNewsSummaryAsync(string articleId, int limit = 3);
        Task<IEnumerable<NewsArticle>> SearchNewsArticlesSummaryAsync(
            string? title = null, 
            string? authorName = null, 
            string? categoryName = null, 
            bool? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    IQueryable<NewsArticle> GetNewsArticlesSummaryQueryable();
        
        // Analytics Methods
        Task<IEnumerable<NewsArticle>> GetTrendingArticlesAsync(int top = 5);
        Task<IEnumerable<NewsArticle>> GetRecommendedArticlesAsync(string articleId, int top = 5);
    }
}