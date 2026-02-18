using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BussinessLogic.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public NewsArticleService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
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
            var article = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Include(n => n.NewsArticleImages)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);
            
            if (article != null)
            {
                // Increment ViewCount
                article.ViewCount++;
                _unitOfWork.NewsArticleRepository.Update(article);
                // We typically want to save this, but keep in mind performance impact.
                // For this assignment scope, saving immediately is fine.
                await _unitOfWork.SaveChangesAsync();
            }

            return article;
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

            // Audit Log
            if (article.CreatedById.HasValue)
            {
                 // Avoid circular reference in serialization by passing anonymous object or handling it in AuditService
                 // Passing article directly might be OK if ReferenceHandler.IgnoreCycles is set
                await _auditService.LogAsync(article.CreatedById.Value, "Create", "NewsArticle", article.NewsArticleId, null, article);
            }

            return article;
        }

        public async Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, IEnumerable<int>? tagIds = null)
        {
            var existingArticle = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .AsNoTracking() // Get a copy for OldValues
                .FirstOrDefaultAsync(n => n.NewsArticleId == article.NewsArticleId);

            if (existingArticle == null)
            {
                throw new InvalidOperationException("Article not found");
            }
            
            // Re-fetch tracked entity to update
             var articleToUpdate = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == article.NewsArticleId);

            // Update properties
            articleToUpdate!.NewsTitle = article.NewsTitle;
            articleToUpdate.Headline = article.Headline;
            articleToUpdate.NewsContent = article.NewsContent;
            articleToUpdate.NewsSource = article.NewsSource;
            articleToUpdate.CategoryId = article.CategoryId;
            articleToUpdate.NewsStatus = article.NewsStatus;
            articleToUpdate.UpdatedById = article.UpdatedById;
            articleToUpdate.ModifiedDate = DateTime.Now;

            // Handle tags
            articleToUpdate.Tags.Clear();
            if (tagIds != null && tagIds.Any())
            {
                var tags = await _unitOfWork.TagRepository.Query()
                    .Where(t => tagIds.Contains(t.TagId))
                    .ToListAsync();
                foreach (var tag in tags)
                {
                    articleToUpdate.Tags.Add(tag);
                }
            }

            _unitOfWork.NewsArticleRepository.Update(articleToUpdate);
            await _unitOfWork.SaveChangesAsync();

             // Audit Log
            if (article.UpdatedById.HasValue)
            {
                await _auditService.LogAsync(article.UpdatedById.Value, "Update", "NewsArticle", article.NewsArticleId, existingArticle, articleToUpdate);
            }

            return articleToUpdate;
        }

        public async Task<bool> DeleteNewsArticleAsync(string id, short? userId = null)
        {
            var article = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);

            if (article == null)
            {
                return false;
            }

            // Capture data for log
            // Re-fetch tracked entity to delete
            var articleToDelete = await _unitOfWork.NewsArticleRepository.Query()
                 .Include(n => n.Tags)
                 .FirstOrDefaultAsync(n => n.NewsArticleId == id);
            
            if (articleToDelete == null) return false;

            // Clear tags relationship
            articleToDelete.Tags.Clear();
            
            _unitOfWork.NewsArticleRepository.Delete(articleToDelete);
            await _unitOfWork.SaveChangesAsync();
            
            // Audit Log
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "Delete", "NewsArticle", id, article, null);
            }
            
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
                var endOfDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(n => n.CreatedDate <= endOfDate);
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
                .Include(n => n.Tags)
                .Include(n => n.NewsArticleImages);
        }

        // Summary methods - exclude NewsContent to reduce payload size
        public async Task<IEnumerable<NewsArticle>> GetActiveNewsArticlesSummaryAsync()
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Include(n => n.NewsArticleImages)
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
                    ViewCount = n.ViewCount,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags,
                    NewsArticleImages = n.NewsArticleImages
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
                    ViewCount = n.ViewCount,
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
                    ViewCount = n.ViewCount,
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
                    ViewCount = n.ViewCount,
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
                var endOfDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(n => n.CreatedDate <= endOfDate);
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
                    ViewCount = n.ViewCount,
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
                    ViewCount = n.ViewCount,
                    Category = n.Category,
                    CreatedBy = n.CreatedBy,
                    Tags = n.Tags
                });
        }


        public async Task<IEnumerable<NewsArticle>> GetTrendingArticlesAsync(int top = 5)
        {
            return await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .OrderByDescending(n => n.ViewCount)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetRecommendedArticlesAsync(string articleId, int top = 5)
        {
             var article = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == articleId);
            
            if (article == null) return new List<NewsArticle>();

            // Recommend based on Category or Tags
            var query = _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.NewsArticleId != articleId && n.NewsStatus == true)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy);

            // 1. Prioritize same category AND shared tags
            // 2. Same category
            // 3. Shared tags
            
            // Simplified approach for EF Core translation:
            // Get candidate articles that match either category or tags
            var tagIds = article.Tags.Select(t => t.TagId).ToList();
            
            var recommendations = await query
                .Where(n => n.CategoryId == article.CategoryId || n.Tags.Any(t => tagIds.Contains(t.TagId)))
                .OrderByDescending(n => n.CategoryId == article.CategoryId) // Prioritize category match
                .ThenByDescending(n => n.ViewCount) // Then popular ones
                .Take(top)
                .ToListAsync();
                
            return recommendations;
        }
    }
}