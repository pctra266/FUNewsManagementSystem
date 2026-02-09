using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;
using Microsoft.AspNetCore.SignalR;
using Presentation_API.Hubs;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class NewsArticlesController : ODataController
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public NewsArticlesController(
            INewsArticleService newsArticleService,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService)
        {
            _newsArticleService = newsArticleService;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing active news
        public async Task<IActionResult> Get()
        {
            try
            {
                var articles = _newsArticleService.GetNewsArticlesQueryable();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving news articles", error = ex.Message });
            }
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing specific article
        public async Task<IActionResult> Get([FromRoute] string key)
        {
            try
            {
                var article = await _newsArticleService.GetNewsArticleByIdAsync(key);
                if (article == null)
                {
                    return NotFound(new { message = $"News article with ID {key} not found" });
                }
                return Ok(article);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the news article", error = ex.Message });
            }
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Recommend([FromRoute] string key)
        {
            try
            {
                var articles = await _newsArticleService.GetRecommendedArticlesAsync(key);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recommended articles", error = ex.Message });
            }
        }

        [HttpGet("odata/NewsArticlesFunctions/Active")]
        [EnableQuery(PageSize = 99, MaxTop = 99)]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var activeArticles = await _newsArticleService.GetActiveNewsArticlesSummaryAsync();
                return Ok(activeArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active news articles", error = ex.Message });
            }
        }

        [HttpGet("odata/NewsArticlesFunctions/Search")]
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
                var articles = await _newsArticleService.SearchNewsArticlesSummaryAsync(
                    title, authorName, categoryName, status, startDate, endDate);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching news articles", error = ex.Message });
            }
        }

        [HttpGet("odata/NewsArticlesFunctions/ByAuthor")]
        [EnableQuery(PageSize = 20, MaxTop = 50)]
        public async Task<IActionResult> GetByAuthor([FromQuery] int authorId)
        {
            try
            {
                var articles = await _newsArticleService.GetNewsArticlesByAuthorSummaryAsync((short)authorId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving articles by author", error = ex.Message });
            }
        }

        [HttpGet("odata/NewsArticlesFunctions/ByCategory")]
        [EnableQuery(PageSize = 20, MaxTop = 50)]
        public async Task<IActionResult> ByCategory([FromQuery] short categoryId)
        {
            try
            {
                var articles = await _newsArticleService.GetNewsArticlesByCategorySummaryAsync(categoryId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving articles by category", error = ex.Message });
            }
        }

        [HttpGet("odata/NewsArticlesFunctions/Related")]
        [EnableQuery(PageSize = 20, MaxTop = 20)]
        [AllowAnonymous]
        public async Task<IActionResult> GetRelated([FromQuery] string articleId, [FromQuery] int limit = 5)
        {
            try
            {
                var relatedArticles = await _newsArticleService.GetRelatedNewsSummaryAsync(articleId, limit);
                return Ok(relatedArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving related articles", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] NewsArticleCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!short.TryParse(userIdClaim, out short userId))
                {
                    return Unauthorized(new { message = "Invalid user identification" });
                }

                var article = new NewsArticle
                {
                    NewsTitle = createDto.NewsTitle,
                    Headline = createDto.Headline,
                    NewsContent = createDto.NewsContent,
                    NewsSource = createDto.NewsSource,
                    CategoryId = createDto.CategoryId,
                    NewsStatus = createDto.NewsStatus ?? true,
                    CreatedById = userId,
                    CreatedDate = DateTime.Now
                };

                var createdArticle = await _newsArticleService.CreateNewsArticleAsync(article, createDto.TagIds);
                
                // Send real-time notification via SignalR
                var notificationMessage = $"📰 New article published: {createdArticle.NewsTitle}";
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", notificationMessage);
                
                // Store notification in service
                _notificationService.AddNotification(notificationMessage);
                
                return Created($"/odata/NewsArticles('{createdArticle.NewsArticleId}')", createdArticle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the news article", error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromRoute] string key, [FromBody] NewsArticleUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user ID for UpdatedById
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!short.TryParse(userIdClaim, out short userId))
                {
                    return Unauthorized(new { message = "Invalid user identification" });
                }

                var article = new NewsArticle
                {
                    NewsArticleId = key,
                    NewsTitle = updateDto.NewsTitle,
                    Headline = updateDto.Headline,
                    NewsContent = updateDto.NewsContent,
                    NewsSource = updateDto.NewsSource,
                    CategoryId = updateDto.CategoryId,
                    NewsStatus = updateDto.NewsStatus,
                    UpdatedById = userId,
                    ModifiedDate = DateTime.Now
                };

                var updatedArticle = await _newsArticleService.UpdateNewsArticleAsync(article, updateDto.TagIds);
                return Ok(updatedArticle);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the news article", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] string key)
        {
            try
            {
                // Get current user ID for audit log
                short? userId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (short.TryParse(userIdClaim, out short parsedId))
                {
                    userId = parsedId;
                }

                var success = await _newsArticleService.DeleteNewsArticleAsync(key, userId);
                if (!success)
                {
                    return NotFound(new { message = $"News article with ID {key} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the news article", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Duplicate([FromRoute] string key)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!short.TryParse(userIdClaim, out short userId))
                {
                    return Unauthorized(new { message = "Invalid user identification" });
                }

                var duplicatedArticle = await _newsArticleService.DuplicateArticleAsync(key, userId);

                return Ok(duplicatedArticle);
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

        [HttpPost("/api/NewsArticles/upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 5MB limit
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return BadRequest("File size exceeds 5MB limit.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Only .jpg, .jpeg, .png, .gif are allowed.");

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";

                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the image", error = ex.Message });
            }
        }
    }
}