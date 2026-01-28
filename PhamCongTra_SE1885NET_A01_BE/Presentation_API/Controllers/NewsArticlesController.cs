using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class NewsArticlesController : ODataController
    {
        private readonly INewsArticleService _newsArticleService;

        public NewsArticlesController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
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
                var success = await _newsArticleService.DeleteNewsArticleAsync(key);
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
    }
}