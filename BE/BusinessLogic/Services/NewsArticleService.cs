using DataAccess.Models;
using Repositories;

namespace Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public NewsArticleService(INewsArticleRepository newsArticleRepository)
        {
            _newsArticleRepository = newsArticleRepository;
        }

        public void AddNewsArticle(NewsArticle newsArticle)
        {
            _newsArticleRepository.AddNewsArticle(newsArticle);
        }

        public bool NewsArticleExists(string id)
        {
            return _newsArticleRepository.NewsArticleExists(id);
        }

        public void DeleteNewsArticle(string id)
        {
            _newsArticleRepository.DeleteNewsArticle(id);
        }

        public List<NewsArticle> GetNewsArticles()
        {
            return _newsArticleRepository.GetNewsArticles();
        }

        public NewsArticle GetNewsArticleById(string id)
        {
            return _newsArticleRepository.GetNewsArticleById(id);
        }

        public List<NewsArticle> GetNewsArticlesByCategory(short categoryId)
        {
            return _newsArticleRepository.GetNewsArticlesByCategory(categoryId);
        }

        public List<NewsArticle> SearchNewsArticles(string keyword)
        {
            return _newsArticleRepository.SearchNewsArticles(keyword);
        }

        public void UpdateNewsArticle(NewsArticle newsArticle)
        {
            _newsArticleRepository.UpdateNewsArticle(newsArticle);
        }

        public List<NewsArticle> GetActiveNewsArticles()
        {
            return _newsArticleRepository.GetActiveNewsArticles();
        }
    }
}
