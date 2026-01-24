using DataAccess.Models;


namespace Repositories
{
        public interface INewsArticleRepository
        {
            List<NewsArticle> GetNewsArticles();
            NewsArticle GetNewsArticleById(string id);
            void AddNewsArticle(NewsArticle newsArticle);
            void UpdateNewsArticle(NewsArticle newsArticle);
            void DeleteNewsArticle(string id);
            public bool NewsArticleExists(string id);
            List<NewsArticle> SearchNewsArticles(string keyword);
            public List<NewsArticle> GetNewsArticlesByCategory(short categoryId);
            public List<NewsArticle> GetActiveNewsArticles();
            public List<NewsArticle> GetNewsArticlesByCreatedBy(short createdById);
            public List<object> GetNewsStatisticsByDateRange(DateTime startDate, DateTime endDate);
    }
}
