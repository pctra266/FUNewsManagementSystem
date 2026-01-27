using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticlesController : ControllerBase
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public NewsArticlesController(INewsArticleRepository newsArticleRepository)
        {
            _newsArticleRepository = newsArticleRepository;
        }

        // GET: api/NewsArticles
        [HttpGet]
        [Authorize(Policy = "LecturerAccess")]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetNewsArticles()
        {
            var newsArticles = await Task.Run(() => _newsArticleRepository.GetNewsArticles());
            return Ok(newsArticles);
        }

        // GET: api/NewsArticles/Statistics?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("Statistics")]
        public async Task<ActionResult<IEnumerable<object>>> GetNewsStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await Task.Run(() => _newsArticleRepository.GetNewsStatisticsByDateRange(startDate, endDate));
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating statistics: {ex.Message}");
            }
        }

        // GET: api/NewsArticles/ByUser/{userId}
        [HttpGet("ByUser/{userId}")]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetNewsArticlesByUser(short userId)
        {
            var newsArticles = await Task.Run(() => _newsArticleRepository.GetNewsArticlesByCreatedBy(userId));
            return Ok(newsArticles);
        }

        // GET: api/NewsArticles/ForUser/{userId}/{isAdmin}
        [HttpGet("ForUser/{userId}/{isAdmin}")]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetNewsArticlesForUser(short userId, bool isAdmin)
        {
            List<NewsArticle> newsArticles;
            
            if (isAdmin)
            {
                // Admin thấy tất cả NewsArticle
                newsArticles = await Task.Run(() => _newsArticleRepository.GetNewsArticles());
            }
            else
            {
                // User chỉ thấy NewsArticle do mình tạo
                newsArticles = await Task.Run(() => _newsArticleRepository.GetNewsArticlesByCreatedBy(userId));
            }
            
            return Ok(newsArticles);
        }

        // GET: api/NewsArticles/5
        [HttpGet("{id}")]
        [Authorize(Policy = "LecturerAccess")]
        public async Task<ActionResult<NewsArticle>> GetNewsArticle(string id)
        {
            var newsArticle = await Task.Run(() => _newsArticleRepository.GetNewsArticleById(id));

            if (newsArticle == null)
            {
                return NotFound();
            }

            return Ok(newsArticle);
        }

        // PUT: api/NewsArticles/5
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffAccess")]
        public async Task<IActionResult> PutNewsArticle(string id, NewsArticle newsArticle)
        {
            if (id != newsArticle.NewsArticleId)
            {
                return BadRequest();
            }

            try
            {
                await Task.Run(() => _newsArticleRepository.UpdateNewsArticle(newsArticle));
            }
            catch (Exception)
            {
                if (!_newsArticleRepository.NewsArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/NewsArticles
        [HttpPost]
        [Authorize(Policy = "StaffAccess")]
        public async Task<ActionResult<NewsArticle>> PostNewsArticle(NewsArticle newsArticle)
        {
            try
            {
                await Task.Run(() => _newsArticleRepository.AddNewsArticle(newsArticle));
            }
            catch (Exception)
            {
                if (_newsArticleRepository.NewsArticleExists(newsArticle.NewsArticleId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetNewsArticle", new { id = newsArticle.NewsArticleId }, newsArticle);
        }

        // DELETE: api/NewsArticles/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffAccess")]
        public async Task<IActionResult> DeleteNewsArticle(string id)
        {
            var newsArticle = await Task.Run(() => _newsArticleRepository.GetNewsArticleById(id));
            if (newsArticle == null)
            {
                return NotFound();
            }

            await Task.Run(() => _newsArticleRepository.DeleteNewsArticle(id));
            return NoContent();
        }

        // GET: api/NewsArticles/public
        [HttpGet("public")]
        [AllowAnonymous] // ✅ Cho phép truy cập không cần JWT
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetPublicNewsArticles()
        {
            var activeNews = await Task.Run(() => 
                _newsArticleRepository.GetActiveNewsArticles());
            return Ok(activeNews);
        }

        // GET: api/NewsArticles/5/related
        [HttpGet("{id}/related")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetRelatedNews(string id)
        {
            var current = await Task.Run(() => _newsArticleRepository.GetNewsArticleById(id));
            if (current == null) return NotFound();
            
            var related = await Task.Run(() => _newsArticleRepository
                .GetNewsArticles()
                .Where(n => n.NewsArticleId != id && 
                            n.NewsStatus == true &&
                            (n.CategoryId == current.CategoryId ||
                             n.Tags.Any(t => current.Tags.Select(ct => ct.TagId).Contains(t.TagId))))
                .Take(3)
                .ToList());
            
            return Ok(related);
        }

        // GET: api/NewsArticles/Search?keyword=tech&categoryId=1&status=true&authorId=2
        [HttpGet("Search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> SearchArticles(
            [FromQuery] string? keyword,
            [FromQuery] short? categoryId,
            [FromQuery] bool? status,
            [FromQuery] short? authorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var articles = await Task.Run(() => _newsArticleRepository.GetNewsArticles());
            
            if (!string.IsNullOrEmpty(keyword))
            {
                articles = articles.Where(a => 
                    (a.NewsTitle != null && a.NewsTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Headline != null && a.Headline.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (a.NewsContent != null && a.NewsContent.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (categoryId.HasValue)
            {
                articles = articles.Where(a => a.CategoryId == categoryId.Value).ToList();
            }
            
            if (status.HasValue)
            {
                articles = articles.Where(a => a.NewsStatus == status.Value).ToList();
            }
            
            if (authorId.HasValue)
            {
                articles = articles.Where(a => a.CreatedById == authorId.Value).ToList();
            }
            
            if (startDate.HasValue)
            {
                articles = articles.Where(a => a.CreatedDate >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                articles = articles.Where(a => a.CreatedDate <= endDate.Value).ToList();
            }
            
            // Sort by CreatedDate descending
            articles = articles.OrderByDescending(a => a.CreatedDate).ToList();
            
            return Ok(articles);
        }

        // POST: api/NewsArticles/5/Duplicate
        [HttpPost("{id}/Duplicate")]
        [Authorize(Policy = "StaffAccess")]
        public async Task<ActionResult<NewsArticle>> DuplicateArticle(string id)
        {
            var original = await Task.Run(() => _newsArticleRepository.GetNewsArticleById(id));
            if (original == null)
            {
                return NotFound(new { message = "Article not found." });
            }
            
            // Tạo bản sao với ID mới
            var duplicate = new NewsArticle
            {
                NewsArticleId = $"{original.NewsArticleId}_COPY_{DateTime.Now:yyyyMMddHHmmss}",
                NewsTitle = $"[COPY] {original.NewsTitle}",
                Headline = original.Headline,
                NewsContent = original.NewsContent,
                NewsSource = original.NewsSource,
                CategoryId = original.CategoryId,
                NewsStatus = false, // Mặc định là inactive
                CreatedById = original.CreatedById,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                UpdatedById = original.CreatedById
            };
            
            try
            {
                await Task.Run(() => _newsArticleRepository.AddNewsArticle(duplicate));
                
                // TODO: Copy tags nếu cần
                
                return CreatedAtAction("GetNewsArticle", new { id = duplicate.NewsArticleId }, duplicate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error duplicating article: {ex.Message}" });
            }
        }
    }
}