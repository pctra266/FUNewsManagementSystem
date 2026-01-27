using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "StaffAccess")]
    public class NewsTagController : ControllerBase
    {
        private readonly NewsContext _context;

        public NewsTagController(NewsContext context)
        {
            _context = context;
        }

        // GET: api/NewsTag/Article/A001
        [HttpGet("Article/{articleId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsByArticle(string articleId)
        {
            var article = await _context.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.NewsArticleId == articleId);
            
            if (article == null)
            {
                return NotFound(new { message = "Article not found." });
            }
            
            return Ok(article.Tags);
        }

        // POST: api/NewsTag/AddTag
        [HttpPost("AddTag")]
        public async Task<IActionResult> AddTagToArticle([FromBody] NewsTagRequest request)
        {
            // Kiểm tra article tồn tại
            var article = await _context.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.NewsArticleId == request.ArticleId);
            
            if (article == null)
            {
                return NotFound(new { message = "Article not found." });
            }
            
            // Kiểm tra tag tồn tại
            var tag = await _context.Tags.FindAsync(request.TagId);
            if (tag == null)
            {
                return NotFound(new { message = "Tag not found." });
            }
            
            // Kiểm tra đã có liên kết chưa
            if (article.Tags.Any(t => t.TagId == request.TagId))
            {
                return BadRequest(new { message = "Tag already added to this article." });
            }
            
            // Thêm tag
            article.Tags.Add(tag);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Tag added successfully." });
        }

        // DELETE: api/NewsTag/RemoveTag
        [HttpDelete("RemoveTag")]
        public async Task<IActionResult> RemoveTagFromArticle([FromBody] NewsTagRequest request)
        {
            var article = await _context.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.NewsArticleId == request.ArticleId);
            
            if (article == null)
            {
                return NotFound(new { message = "Article not found." });
            }
            
            var tag = article.Tags.FirstOrDefault(t => t.TagId == request.TagId);
            if (tag == null)
            {
                return NotFound(new { message = "Tag not found in this article." });
            }
            
            article.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Tag removed successfully." });
        }

        // PUT: api/NewsTag/UpdateArticleTags
        [HttpPut("UpdateArticleTags")]
        public async Task<IActionResult> UpdateArticleTags([FromBody] UpdateArticleTagsRequest request)
        {
            var article = await _context.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.NewsArticleId == request.ArticleId);
            
            if (article == null)
            {
                return NotFound(new { message = "Article not found." });
            }
            
            // Xóa tất cả tags hiện tại
            article.Tags.Clear();
            
            // Thêm tags mới
            if (request.TagIds != null && request.TagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => request.TagIds.Contains(t.TagId))
                    .ToListAsync();
                
                foreach (var tag in tags)
                {
                    article.Tags.Add(tag);
                }
            }
            
            await _context.SaveChangesAsync();
            
            return Ok(new { 
                message = "Article tags updated successfully.",
                tagCount = article.Tags.Count
            });
        }
    }

    // DTOs
    public class NewsTagRequest
    {
        public string ArticleId { get; set; }
        public int TagId { get; set; }
    }

    public class UpdateArticleTagsRequest
    {
        public string ArticleId { get; set; }
        public List<int> TagIds { get; set; }
    }
}