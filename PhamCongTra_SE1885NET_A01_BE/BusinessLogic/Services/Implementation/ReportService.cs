using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;
using ClosedXML.Excel;
using DataAccess.DTOs;

namespace BussinessLogic.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> GetArticleStatisticsByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var articles = await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate)
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            var statistics = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                TotalArticles = articles.Count,
                ActiveArticles = articles.Count(a => a.NewsStatus == true),
                InactiveArticles = articles.Count(a => a.NewsStatus == false),
                Articles = articles.Select(a => new
                {
                    a.NewsArticleId,
                    a.NewsTitle,
                    a.CreatedDate,
                    CategoryName = a.Category?.CategoryName,
                    AuthorName = a.CreatedBy?.AccountName,
                    Status = a.NewsStatus == true ? "Active" : "Inactive"
                }).ToList()
            };

            return statistics;
        }

        public async Task<CategoryReportDto> GetArticleStatisticsByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, bool? status = null)
        {
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category);

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedDate <= endDate);

            if (status.HasValue)
                query = query.Where(n => n.NewsStatus == status);

            var articles = await query.ToListAsync();

            var categoryStats = articles
                .GroupBy(a => new { a.CategoryId, CategoryName = a.Category?.CategoryName ?? "Uncategorized" })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(a => a.NewsStatus == true),
                    InactiveArticles = g.Count(a => a.NewsStatus == false),
                    LatestArticle = g.Max(a => a.CreatedDate)
                })
                .OrderByDescending(s => s.TotalArticles)
                .ToList();

            return new CategoryReportDto
            {
                Period = new PeriodDto { StartDate = startDate, EndDate = endDate },
                CategoryStatistics = categoryStats.Select(s => new CategoryStatisticDto
                {
                    CategoryId = (int?)s.CategoryId,
                    CategoryName = s.CategoryName,
                    TotalArticles = s.TotalArticles,
                    ActiveArticles = s.ActiveArticles,
                    InactiveArticles = s.InactiveArticles,
                    LatestArticle = s.LatestArticle
                }).ToList()
            };
        }

        public async Task<AuthorReportDto> GetArticleStatisticsByAuthorAsync(DateTime? startDate = null, DateTime? endDate = null, bool? status = null)
        {
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.CreatedBy);

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedDate <= endDate);

            if (status.HasValue)
                query = query.Where(n => n.NewsStatus == status);

            var articles = await query.ToListAsync();

            var authorStats = articles
                .GroupBy(a => new { a.CreatedById, AuthorName = a.CreatedBy?.AccountName ?? "Unknown" })
                .Select(g => new
                {
                    AuthorId = g.Key.CreatedById,
                    AuthorName = g.Key.AuthorName,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(a => a.NewsStatus == true),
                    InactiveArticles = g.Count(a => a.NewsStatus == false),
                    LatestArticle = g.Max(a => a.CreatedDate),
                    FirstArticle = g.Min(a => a.CreatedDate)
                })
                .OrderByDescending(s => s.TotalArticles)
                .ToList();

            return new AuthorReportDto
            {
                Period = new PeriodDto { StartDate = startDate, EndDate = endDate },
                AuthorStatistics = authorStats.Select(s => new AuthorStatisticDto
                {
                    AuthorId = (int?)s.AuthorId,
                    AuthorName = s.AuthorName,
                    TotalArticles = s.TotalArticles,
                    ActiveArticles = s.ActiveArticles,
                    InactiveArticles = s.InactiveArticles,
                    LatestArticle = s.LatestArticle,
                    FirstArticle = s.FirstArticle
                }).ToList()
            };
        }

        public async Task<StatusReportDto> GetArticleStatisticsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query();

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedDate <= endDate);

            var articles = await query.ToListAsync();

            var statusStats = new
            {
                TotalArticles = articles.Count,
                ActiveArticles = articles.Count(a => a.NewsStatus == true),
                InactiveArticles = articles.Count(a => a.NewsStatus == false),
                ActivePercentage = articles.Count > 0 ? 
                    Math.Round((double)articles.Count(a => a.NewsStatus == true) / articles.Count * 100, 2) : 0,
                InactivePercentage = articles.Count > 0 ? 
                    Math.Round((double)articles.Count(a => a.NewsStatus == false) / articles.Count * 100, 2) : 0
            };

            return new StatusReportDto
            {
                Period = new PeriodDto { StartDate = startDate, EndDate = endDate },
                StatusStatistics = new StatusStatisticDto
                {
                    TotalArticles = statusStats.TotalArticles,
                    ActiveArticles = statusStats.ActiveArticles,
                    InactiveArticles = statusStats.InactiveArticles,
                    ActivePercentage = statusStats.ActivePercentage,
                    InactivePercentage = statusStats.InactivePercentage
                }
            };
        }

        public async Task<object> GetCategoryUsageStatisticsAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.Query()
                .Include(c => c.NewsArticles)
                .ToListAsync();

            var categoryUsage = categories.Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.IsActive,
                ArticleCount = c.NewsArticles.Count,
                ActiveArticleCount = c.NewsArticles.Count(a => a.NewsStatus == true),
                InactiveArticleCount = c.NewsArticles.Count(a => a.NewsStatus == false)
            })
            .OrderByDescending(c => c.ArticleCount)
            .ToList();

            return new
            {
                TotalCategories = categories.Count,
                ActiveCategories = categories.Count(c => c.IsActive == true),
                UnusedCategories = categories.Count(c => c.NewsArticles.Count == 0),
                CategoryUsage = categoryUsage
            };
        }

        public async Task<object> GetDashboardStatisticsAsync()
        {
            var articles = await _unitOfWork.NewsArticleRepository.GetAllAsync();
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var accounts = await _unitOfWork.AccountRepository.GetAllAsync();
            var tags = await _unitOfWork.TagRepository.GetAllAsync();

            var recentArticles = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .ToListAsync();

            // Monthly statistics for current year
            var currentYear = DateTime.Now.Year;
            var monthlyStats = await GetMonthlyArticleStatsAsync(currentYear);

            return new
            {
                TotalArticles = articles.Count(),
                ActiveArticles = articles.Count(a => a.NewsStatus == true),
                InactiveArticles = articles.Count(a => a.NewsStatus == false),
                TotalCategories = categories.Count(),
                ActiveCategories = categories.Count(c => c.IsActive == true),
                TotalAccounts = accounts.Count(),
                StaffAccounts = accounts.Count(a => a.AccountRole == 1),
                LecturerAccounts = accounts.Count(a => a.AccountRole == 2),
                TotalTags = tags.Count(),
                MonthlyStats = monthlyStats,
                RecentArticles = recentArticles.Select(a => new
                {
                    a.NewsArticleId,
                    a.NewsTitle,
                    a.CreatedDate,
                    CategoryName = a.Category?.CategoryName,
                    AuthorName = a.CreatedBy?.AccountName,
                    Status = a.NewsStatus == true ? "Active" : "Inactive"
                }).ToList()
            };
        }

        public async Task<object> GetMonthlyArticleStatsAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            var articles = await _unitOfWork.NewsArticleRepository.Query()
                .Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate)
                .ToListAsync();

            var monthlyStats = articles
                .GroupBy(a => new { Year = a.CreatedDate!.Value.Year, Month = a.CreatedDate!.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(a => a.NewsStatus == true),
                    InactiveArticles = g.Count(a => a.NewsStatus == false)
                })
                .OrderBy(s => s.Month)
                .ToList();

            return new
            {
                Year = year,
                MonthlyStatistics = monthlyStats,
                YearTotal = monthlyStats.Sum(m => m.TotalArticles),
                YearActive = monthlyStats.Sum(m => m.ActiveArticles),
                YearInactive = monthlyStats.Sum(m => m.InactiveArticles)
            };
        }

        public async Task<object> GetTopAuthorsAsync(int limit = 10)
        {
            var authors = await _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.CreatedBy)
                .GroupBy(n => n.CreatedBy)
                .Select(g => new
                {
                    AuthorId = g.Key!.AccountId,
                    AuthorName = g.Key.AccountName,
                    AuthorEmail = g.Key.AccountEmail,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(a => a.NewsStatus == true),
                    InactiveArticles = g.Count(a => a.NewsStatus == false),
                    LatestArticle = g.Max(a => a.CreatedDate),
                    FirstArticle = g.Min(a => a.CreatedDate)
                })
                .OrderByDescending(a => a.TotalArticles)
                .Take(limit)
                .ToListAsync();

            return new
            {
                TopAuthors = authors,
                Limit = limit
            };
        }

        public async Task<object> GetTopCategoriesAsync(int limit = 10)
        {
            var categories = await _unitOfWork.CategoryRepository.Query()
                .Include(c => c.NewsArticles)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.CategoryDescription,
                    c.IsActive,
                    TotalArticles = c.NewsArticles.Count,
                    ActiveArticles = c.NewsArticles.Count(a => a.NewsStatus == true),
                    InactiveArticles = c.NewsArticles.Count(a => a.NewsStatus == false),
                    LatestArticle = c.NewsArticles.Max(a => a.CreatedDate)
                })
                .Where(c => c.TotalArticles > 0)
                .OrderByDescending(c => c.TotalArticles)
                .Take(limit)
                .ToListAsync();

            return new
            {
                TopCategories = categories,
                Limit = limit
            };
        }

        public async Task<object> GetTagUsageStatisticsAsync()
        {
            var tags = await _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles)
                .Select(t => new
                {
                    t.TagId,
                    t.TagName,
                    t.Note,
                    ArticleCount = t.NewsArticles.Count,
                    ActiveArticleCount = t.NewsArticles.Count(a => a.NewsStatus == true),
                    InactiveArticleCount = t.NewsArticles.Count(a => a.NewsStatus == false)
                })
                .OrderByDescending(t => t.ArticleCount)
                .ToListAsync();

            var totalTags = tags.Count;
            var usedTags = tags.Count(t => t.ArticleCount > 0);
            var unusedTags = tags.Count(t => t.ArticleCount == 0);

            return new
            {
                TotalTags = totalTags,
                UsedTags = usedTags,
                UnusedTags = unusedTags,
                UsagePercentage = totalTags > 0 ? Math.Round((double)usedTags / totalTags * 100, 2) : 0,
                TagStatistics = tags
            };
        }


        public async Task<byte[]> ExportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // 1. Fetch Data
            IQueryable<NewsArticle> query = _unitOfWork.NewsArticleRepository.Query()
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags);

            if (startDate.HasValue) query = query.Where(n => n.CreatedDate >= startDate);
            if (endDate.HasValue) query = query.Where(n => n.CreatedDate <= endDate);

            var articles = await query
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            // 2. Create Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Article Report");

                // Headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Title";
                worksheet.Cell(1, 3).Value = "Category";
                worksheet.Cell(1, 4).Value = "Author";
                worksheet.Cell(1, 5).Value = "Created Date";
                worksheet.Cell(1, 6).Value = "Status";
                worksheet.Cell(1, 7).Value = "View Count";
                
                // Style Header
                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var article in articles)
                {
                    worksheet.Cell(row, 1).Value = article.NewsArticleId;
                    worksheet.Cell(row, 2).Value = article.NewsTitle;
                    worksheet.Cell(row, 3).Value = article.Category?.CategoryName ?? "N/A";
                    worksheet.Cell(row, 4).Value = article.CreatedBy?.AccountName ?? "Unknown";
                    worksheet.Cell(row, 5).Value = article.CreatedDate?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 6).Value = article.NewsStatus == true ? "Active" : "Inactive";
                    worksheet.Cell(row, 7).Value = article.ViewCount;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}