using DataAccess.Models;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class NewsArticleRepository : INewsArticleRepository
    {
        private readonly NewsContext _context;

        public NewsArticleRepository(NewsContext context)
        {
            _context = context;
        }

        public List<NewsArticle> GetNewsArticles()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public void AddNewsArticle(NewsArticle newsArticle)
        {
            _context.NewsArticles.Add(newsArticle);
            _context.SaveChanges();
        }

        public void UpdateNewsArticle(NewsArticle newsArticle)
        {
            _context.NewsArticles.Update(newsArticle);
            _context.SaveChanges();
        }

        public void DeleteNewsArticle(string id)
        {
            // Tìm article bao gồm cả collection Tags để xử lý Many-to-Many
            var article = _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == id);

            if (article != null)
            {
                // 1. Xóa các liên kết trong bảng trung gian trước (Junction Table)
                article.Tags.Clear();
                _context.SaveChanges();

                // 2. Xóa bài viết chính
                _context.NewsArticles.Remove(article);
                _context.SaveChanges();
            }
        }

        public NewsArticle GetNewsArticleById(string id)
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == id);
        }

        public bool NewsArticleExists(string id)
        {
            return _context.NewsArticles.Any(n => n.NewsArticleId == id);
        }

        public List<NewsArticle> SearchNewsArticles(string keyword)
        {
            return _context.NewsArticles
                .Where(n => n.NewsTitle.Contains(keyword) || (n.NewsContent != null && n.NewsContent.Contains(keyword)))
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public List<NewsArticle> GetActiveNewsArticles()
        {
            return _context.NewsArticles
                .Where(n => n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public List<NewsArticle> GetNewsArticlesByCategory(short categoryId)
        {
            return _context.NewsArticles
                .Where(n => n.CategoryId == categoryId) // Sửa logic so sánh ID
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public List<NewsArticle> GetNewsArticlesByCreatedBy(short createdById)
        {
            return _context.NewsArticles
                .Where(n => n.CreatedById == createdById)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public List<object> GetNewsStatisticsByDateRange(DateTime startDate, DateTime endDate)
        {
            var articles = _context.NewsArticles
                .Where(n => n.CreatedDate.HasValue &&
                            n.CreatedDate.Value.Date >= startDate.Date &&
                            n.CreatedDate.Value.Date <= endDate.Date)
                .ToList();

            var statistics = articles
                .GroupBy(n => n.CreatedDate.Value.Date)
                .Select(g => new {
                    Date = g.Key,
                    DateString = g.Key.ToString("yyyy-MM-dd"),
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(n => n.NewsStatus == true),
                    InactiveArticles = g.Count(n => n.NewsStatus == false)
                })
                .OrderByDescending(s => s.Date)
                .Cast<object>()
                .ToList();

            return statistics;
        }
    }
}