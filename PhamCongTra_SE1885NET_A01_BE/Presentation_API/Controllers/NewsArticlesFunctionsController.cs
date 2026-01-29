using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Route("odata/[controller]")]
    public class NewsArticlesFunctionsController : ODataController
    {
        private readonly INewsArticleService _newsArticleService;

        public NewsArticlesFunctionsController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        [HttpGet("Active")]
        [EnableQuery(PageSize = 99, MaxTop = 99)] // Giới hạn 7 items per page
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                // Use summary method to exclude full content
                var activeArticles = await _newsArticleService.GetActiveNewsArticlesSummaryAsync();
                return Ok(activeArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active news articles", error = ex.Message });
            }
        }

        [HttpGet("Search")]
        [EnableQuery(PageSize = 20, MaxTop = 50)]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            [FromQuery] string? title,
            [FromQuery] string? authorName,
            [FromQuery] string? categoryName,
            [FromQuery] bool? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                // Use summary method to exclude full content
                var articles = await _newsArticleService.SearchNewsArticlesSummaryAsync(
                    title, authorName, categoryName, status, startDate, endDate);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching news articles", error = ex.Message });
            }
        }

        [HttpPost("Duplicate")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> DuplicateArticle([FromBody] DuplicateArticleRequest request)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!short.TryParse(userIdClaim, out short userId))
                {
                    return Unauthorized(new { message = "Invalid user identification" });
                }

                var duplicatedArticle = await _newsArticleService.DuplicateArticleAsync(request.ArticleId, userId);

                return Ok(new
                {
                    message = "Article duplicated successfully",
                    articleId = duplicatedArticle.NewsArticleId,
                    article = duplicatedArticle
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while duplicating the article", error = ex.Message });
            }
        }

        [HttpGet("ByAuthor")]
        [EnableQuery(PageSize = 20, MaxTop = 50)]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> GetByAuthor([FromQuery] int authorId)
        {
            try
            {
                // Use summary method to exclude full content
                var articles = await _newsArticleService.GetNewsArticlesByAuthorSummaryAsync((short)authorId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving articles by author", error = ex.Message });
            }
        }

        [HttpGet("ByCategory")]
        [EnableQuery(PageSize = 20, MaxTop = 50)]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory([FromQuery] short categoryId)
        {
            try
            {
                // Use summary method to exclude full content
                var articles = await _newsArticleService.GetNewsArticlesByCategorySummaryAsync(categoryId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving articles by category", error = ex.Message });
            }
        }

        [HttpGet("Related")]
        [EnableQuery(PageSize = 20, MaxTop = 20)] // Keep smaller limit for related articles
        [AllowAnonymous]
        public async Task<IActionResult> GetRelated([FromQuery] string articleId, [FromQuery] int limit = 5)
        {
            try
            {
                // Use summary method to exclude full content
                var relatedArticles = await _newsArticleService.GetRelatedNewsSummaryAsync(articleId, limit);
                return Ok(relatedArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving related articles", error = ex.Message });
            }
        }
        
    }
    public class DuplicateArticleRequest
    {
        public string ArticleId { get; set; } = string.Empty;
    }
}