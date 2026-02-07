using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly INewsArticleService _newsArticleService;

        public ReportsController(IReportService reportService, INewsArticleService newsArticleService)
        {
            _reportService = reportService;
            _newsArticleService = newsArticleService;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var statistics = await _reportService.GetDashboardStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics", error = ex.Message });
            }
        }

        [HttpGet("ArticlesByPeriod")]
        public async Task<IActionResult> GetArticleStatisticsByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new { message = "Start date cannot be greater than end date" });
                }

                var statistics = await _reportService.GetArticleStatisticsByPeriodAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving article statistics by period", error = ex.Message });
            }
        }

        [HttpGet("ArticlesByCategory")]
        public async Task<IActionResult> GetArticleStatisticsByCategory(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? status)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date cannot be greater than end date" });
                }

                var statistics = await _reportService.GetArticleStatisticsByCategoryAsync(startDate, endDate, status);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving article statistics by category", error = ex.Message });
            }
        }

        [HttpGet("ArticlesByAuthor")]
        public async Task<IActionResult> GetArticleStatisticsByAuthor(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool? status)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date cannot be greater than end date" });
                }

                var statistics = await _reportService.GetArticleStatisticsByAuthorAsync(startDate, endDate, status);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving article statistics by author", error = ex.Message });
            }
        }

        [HttpGet("ArticlesByStatus")]
        public async Task<IActionResult> GetArticleStatisticsByStatus(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date cannot be greater than end date" });
                }

                var statistics = await _reportService.GetArticleStatisticsByStatusAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving article statistics by status", error = ex.Message });
            }
        }

        [HttpGet("CategoryUsage")]
        public async Task<IActionResult> GetCategoryUsageStatistics()
        {
            try
            {
                var statistics = await _reportService.GetCategoryUsageStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving category usage statistics", error = ex.Message });
            }
        }

        [HttpGet("TagUsage")]
        public async Task<IActionResult> GetTagUsageStatistics()
        {
            try
            {
                var statistics = await _reportService.GetTagUsageStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving tag usage statistics", error = ex.Message });
            }
        }

        [HttpGet("MonthlyStats")]
        public async Task<IActionResult> GetMonthlyArticleStats([FromQuery] int year = 0)
        {
            try
            {
                if (year == 0)
                {
                    year = DateTime.Now.Year;
                }

                if (year < 2000 || year > DateTime.Now.Year + 1)
                {
                    return BadRequest(new { message = "Invalid year provided" });
                }

                var statistics = await _reportService.GetMonthlyArticleStatsAsync(year);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving monthly article statistics", error = ex.Message });
            }
        }

        [HttpGet("TopAuthors")]
        public async Task<IActionResult> GetTopAuthors([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 100" });
                }

                var statistics = await _reportService.GetTopAuthorsAsync(limit);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving top authors", error = ex.Message });
            }
        }

        [HttpGet("TopCategories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 100" });
                }

                var statistics = await _reportService.GetTopCategoriesAsync(limit);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving top categories", error = ex.Message });
            }
        }


        [HttpGet("Trending")]
        public async Task<IActionResult> GetTrendingArticles([FromQuery] int top = 5)
        {
            try
            {
                var articles = await _newsArticleService.GetTrendingArticlesAsync(top);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving trending articles", error = ex.Message });
            }
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var content = await _reportService.ExportToExcelAsync(startDate, endDate);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"ArticleReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting report", error = ex.Message });
            }
        }
    }
}