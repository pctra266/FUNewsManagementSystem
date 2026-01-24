using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Models;
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
    }
}