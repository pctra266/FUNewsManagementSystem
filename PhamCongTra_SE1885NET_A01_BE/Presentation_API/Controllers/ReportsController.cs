using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using BussinessLogic.Services;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ReportsController : ODataController
    {
        private readonly IReportService _reportService;
        private readonly INewsArticleService _newsArticleService;

        public ReportsController(IReportService reportService, INewsArticleService newsArticleService)
        {
            _reportService = reportService;
            _newsArticleService = newsArticleService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
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

        //[HttpGet]
        //public async Task<CategoryReportDto> ArticlesByCategory(
        //    [FromODataUri] DateTime? startDate,
        //    [FromODataUri] DateTime? endDate,
        //    [FromODataUri] bool? status)
        //{
        //    try
        //    {
        //        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        //        {
        //            // For OData simplicity, we might want to return empty report or throw
        //            return new CategoryReportDto();
        //        }

        //        return await _reportService.GetArticleStatisticsByCategoryAsync(startDate, endDate, status);
        //    }
        //    catch (Exception)
        //    {
        //        return new CategoryReportDto();
        //    }
        //}

        //[HttpGet]
        //public async Task<AuthorReportDto> ArticlesByAuthor(
        //    [FromODataUri] DateTime? startDate,
        //    [FromODataUri] DateTime? endDate,
        //    [FromODataUri] bool? status)
        //{
        //    try
        //    {
        //        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        //        {
        //            return new AuthorReportDto();
        //        }

        //        return await _reportService.GetArticleStatisticsByAuthorAsync(startDate, endDate, status);
        //    }
        //    catch (Exception)
        //    {
        //        return new AuthorReportDto();
        //    }
        //}

        //[HttpGet]
        //public async Task<StatusReportDto> ArticlesByStatus(
        //    [FromODataUri] DateTime? startDate,
        //    [FromODataUri] DateTime? endDate)
        //{
        //    try
        //    {
        //        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        //        {
        //            return new StatusReportDto();
        //        }

        //        return await _reportService.GetArticleStatisticsByStatusAsync(startDate, endDate);
        //    }
        //    catch (Exception)
        //    {
        //        return new StatusReportDto();
        //    }
        //}

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

        [HttpGet]
        public async Task<IActionResult> Trending([FromODataUri] int? top)
        {
            try
            {
                var articles = await _newsArticleService.GetTrendingArticlesAsync(top ?? 5);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving trending articles", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            try
            {
                var content = await _reportService.ExportToExcelAsync(null, null);
                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting report", error = ex.Message });
            }
        }
    }
}