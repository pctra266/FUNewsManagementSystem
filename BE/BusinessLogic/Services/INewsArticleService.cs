using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface INewsArticleService
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
    }
}
