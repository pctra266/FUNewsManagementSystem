using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BussinessLogic.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync()
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        }

        public async Task<IEnumerable<NewsArticle>> GetActiveNewsArticlesAsync()
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsArticlesByAuthorAsync(short authorId)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CreatedById == authorId)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsArticlesByCategoryAsync(short categoryId)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CategoryId == categoryId)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<NewsArticle> CreateNewsArticleAsync(NewsArticle article, IEnumerable<int>? tagIds = null)
        {
            article.NewsArticleId = GenerateNewsArticleId();
            article.CreatedDate = DateTime.Now;

            // Handle tags
            if (tagIds != null && tagIds.Any())
            {
                var tags = await _unitOfWork.TagRepository.Query()
                    .Where(t => tagIds.Contains(t.TagId))
                    .ToListAsync();
                article.Tags = tags;
            }

            await _unitOfWork.NewsArticleRepository.AddAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return article;
        }

        public async Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, IEnumerable<int>? tagIds = null)
        {
            var existingArticle = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == article.NewsArticleId);

            if (existingArticle == null)
            {
                throw new InvalidOperationException("Article not found");
            }

            // Update properties
            existingArticle.NewsTitle = article.NewsTitle;
            existingArticle.Headline = article.Headline;
            existingArticle.NewsContent = article.NewsContent;
            existingArticle.NewsSource = article.NewsSource;
            existingArticle.CategoryId = article.CategoryId;
            existingArticle.NewsStatus = article.NewsStatus;
            existingArticle.UpdatedById = article.UpdatedById;
            existingArticle.ModifiedDate = DateTime.Now;

            // Handle tags
            existingArticle.Tags.Clear();
            if (tagIds != null && tagIds.Any())
            {
                var tags = await _unitOfWork.TagRepository.Query()
                    .Where(t => tagIds.Contains(t.TagId))
                    .ToListAsync();
                foreach (var tag in tags)
                {
                    existingArticle.Tags.Add(tag);
                }
            }

            _unitOfWork.NewsArticleRepository.Update(existingArticle);
            await _unitOfWork.SaveChangesAsync();

            return existingArticle;
        }

        public async Task<bool> DeleteNewsArticleAsync(string id)
        {
            var article = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);

            if (article == null)
            {
                return false;
            }

            // Clear tags relationship
            article.Tags.Clear();
            
            _unitOfWork.NewsArticleRepository.Delete(article);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<NewsArticle>> SearchNewsArticlesAsync(
            string? title = null, 
            string? authorName = null, 
            string? categoryName = null, 
            bool? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags);

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(n => n.NewsTitle!.Contains(title));
            }

            if (!string.IsNullOrEmpty(authorName))
            {
                query = query.Where(n => n.CreatedBy!.AccountName!.Contains(authorName));
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(n => n.Category!.CategoryName!.Contains(categoryName));
            }

            if (status.HasValue)
            {
                query = query.Where(n => n.NewsStatus == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate <= endDate);
            }

            return await query.OrderByDescending(n => n.CreatedDate).ToListAsync();
        }

        public async Task<NewsArticle> DuplicateArticleAsync(string originalId, short newAuthorId)
        {
            var original = await GetNewsArticleByIdAsync(originalId);
            if (original == null)
            {
                throw new InvalidOperationException("Original article not found");
            }

            var duplicate = new NewsArticle
            {
                NewsArticleId = GenerateNewsArticleId(),
                NewsTitle = $"Copy of {original.NewsTitle}",
                Headline = original.Headline,
                NewsContent = original.NewsContent,
                NewsSource = original.NewsSource,
                CategoryId = original.CategoryId,
                NewsStatus = false, // Set as inactive by default
                CreatedById = newAuthorId,
                CreatedDate = DateTime.Now
            };

            // Copy tags
            var tagIds = original.Tags.Select(t => t.TagId).ToList();
            return await CreateNewsArticleAsync(duplicate, tagIds);
        }

        public async Task<IEnumerable<NewsArticle>> GetRelatedNewsAsync(string articleId, int limit = 3)
        {
            var currentArticle = await GetNewsArticleByIdAsync(articleId);
            if (currentArticle == null)
            {
                return new List<NewsArticle>();
            }

            var relatedByCategory = await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CategoryId == currentArticle.CategoryId && 
                           n.NewsArticleId != articleId && 
                           n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Take(limit)
                .ToListAsync();

            if (relatedByCategory.Count >= limit)
            {
                return relatedByCategory;
            }

            // If not enough related by category, get by tags
            var currentTagIds = currentArticle.Tags.Select(t => t.TagId).ToList();
            var relatedByTags = await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsArticleId != articleId && 
                           n.NewsStatus == true &&
                           n.Tags.Any(t => currentTagIds.Contains(t.TagId)))
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Take(limit - relatedByCategory.Count)
                .ToListAsync();

            return relatedByCategory.Concat(relatedByTags);
        }

        public string GenerateNewsArticleId()
        {
            return $"NEWS{DateTime.Now:yyyyMMddHHmmss}";
        }

        public IQueryable<NewsArticle> GetNewsArticlesQueryable()
        {
            return _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags);
        }

        // Summary methods - exclude NewsContent to reduce payload size
        public async Task<IEnumerable<NewsArticle>> GetActiveNewsArticlesSummaryAsync()
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                    // NewsContent is intentionally excluded
                })
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsArticlesByAuthorSummaryAsync(short authorId)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CreatedById == authorId)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                })
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsArticlesByCategorySummaryAsync(short categoryId)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CategoryId == categoryId)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                })
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetRelatedNewsSummaryAsync(string articleId, int limit = 3)
        {
            var currentArticle = await GetNewsArticleByIdAsync(articleId);
            if (currentArticle == null)
            {
                return new List<NewsArticle>();
            }

            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CategoryId == currentArticle.CategoryId && 
                           n.NewsArticleId != articleId && 
                           n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                })
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> SearchNewsArticlesSummaryAsync(
            string? title = null, 
            string? authorName = null, 
            string? categoryName = null, 
            bool? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags);

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(n => n.NewsTitle!.Contains(title));
            }

            if (!string.IsNullOrEmpty(authorName))
            {
                query = query.Where(n => n.CreatedBy!.AccountName!.Contains(authorName));
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(n => n.Category!.CategoryName!.Contains(categoryName));
            }

            if (status.HasValue)
            {
                query = query.Where(n => n.NewsStatus == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate <= endDate);
            }

            return await query
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                })
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public IQueryable<NewsArticle> GetNewsArticlesSummaryQueryable()
        {
            return _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Select(n => new NewsArticle
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId,
                    NewsStatus = n.NewsStatus,
                    CreatedById = n.CreatedById,
                    CreatedDate = n.CreatedDate,
                    ModifiedDate = n.ModifiedDate,
                    UpdatedById = n.UpdatedById,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                });
        }
    }
}