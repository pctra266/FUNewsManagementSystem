using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;
using System.Web;

namespace Presentation_RazorPage.Pages.Admin
{
    public class ReportsModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly IExcelExportService _excelExportService;

        public ReportsModel(IApiService apiService, IExcelExportService excelExportService)
        {
            _apiService = apiService;
            _excelExportService = excelExportService;
        }

        public List<CategoryStatisticModel> CategoryStats { get; set; } = new List<CategoryStatisticModel>();
        public List<AuthorStatisticModel> AuthorStats { get; set; } = new List<AuthorStatisticModel>();
        public List<NewsArticleModel> ArticleDetails { get; set; } = new List<NewsArticleModel>();

        public int TotalArticles { get; set; }
        public int TotalActiveArticles { get; set; }
        public int TotalInactiveArticles { get; set; }
        public int DebugTotalCount { get; set; } // For debugging

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string GroupBy { get; set; } = "category";

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public bool? Status { get; set; }

        private const string ExpandClause = "$expand=Category($select=CategoryName),CreatedBy($select=AccountName),Tags";
        private const string CategoryReportEndpoint = "/odata/NewsArticles/Default.ArticlesByCategory";
        private const string AuthorReportEndpoint = "/odata/NewsArticles/ArticlesByAuthor";

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            //_apiService.SetAuthToken(token);

            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                EndDate = DateTime.Today;
                StartDate = new DateTime(2026, 1, 1);
            }

            try
            {
                await LoadReportDataAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading report data: {ex.Message}";
                CategoryStats = new List<CategoryStatisticModel>();
                AuthorStats = new List<AuthorStatisticModel>();
                ArticleDetails = new List<NewsArticleModel>();
                TotalArticles = 0;
                TotalActiveArticles = 0;
                TotalInactiveArticles = 0;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            //_apiService.SetAuthToken(token);

            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                EndDate = DateTime.Today;
                StartDate = new DateTime(2020, 1, 1);
            }

            try
            {
                await LoadReportDataAsync();

                byte[] excelData;
                string fileName;

                if (GroupBy.Equals("author", StringComparison.OrdinalIgnoreCase))
                {
                    excelData = _excelExportService.ExportAuthorReport(AuthorStats, ArticleDetails, StartDate, EndDate);
                    fileName = $"Author_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
                else
                {
                    excelData = _excelExportService.ExportCategoryReport(CategoryStats, ArticleDetails, StartDate, EndDate);
                    fileName = $"Category_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating Excel export: {ex.Message}";
                return RedirectToPage(new { GroupBy, StartDate, EndDate, SortOrder });
            }
        }

        private async Task LoadReportDataAsync()
        {
            await LoadArticleDetailsAsync();

            // Debug check: Get total articles without any filter
            var rawArticles = await _apiService.GetAsync<NewsArticleModel>("/odata/NewsArticles");
            DebugTotalCount = rawArticles?.Count ?? 0;

            if (GroupBy.Equals("author", StringComparison.OrdinalIgnoreCase))
            {
                await LoadAuthorStatsAsync();
            }
            else
            {
                await LoadCategoryStatsAsync();
            }

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            if (GroupBy.Equals("author", StringComparison.OrdinalIgnoreCase))
            {
                TotalArticles = AuthorStats.Sum(a => a.TotalArticles);
                TotalActiveArticles = AuthorStats.Sum(a => a.ActiveArticles);
                TotalInactiveArticles = AuthorStats.Sum(a => a.InactiveArticles);
            }
            else
            {
                TotalArticles = CategoryStats.Sum(c => c.TotalArticles);
                TotalActiveArticles = CategoryStats.Sum(c => c.ActiveArticles);
                TotalInactiveArticles = CategoryStats.Sum(c => c.InactiveArticles);
            }
        }

        private async Task LoadArticleDetailsAsync()
        {
            try
            {
                var (endpoint, query) = BuildArticleEndpoint();
                var url = $"{endpoint}?{query}&{ExpandClause}".TrimEnd('?');

                var response = await _apiService.GetAsync<NewsArticleModel>(url);
                ArticleDetails = response?.ToList() ?? new List<NewsArticleModel>();
                ArticleDetails.ForEach(a => a.HydrateMetadata());

                ArticleDetails = SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? ArticleDetails.OrderBy(a => a.CreatedDate).ToList()
                    : ArticleDetails.OrderByDescending(a => a.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading article details: {ex.Message}";
                ArticleDetails = new List<NewsArticleModel>();
            }
        }

        private (string Endpoint, string Query) BuildArticleEndpoint()
        {
            var filters = new List<string>();

            if (StartDate.HasValue)
            {
                filters.Add($"startDate={StartDate:yyyy-MM-dd}");
            }

            if (EndDate.HasValue)
            {
                filters.Add($"endDate={EndDate:yyyy-MM-dd}");
            }

            if (Status.HasValue)
            {
                filters.Add($"status={(Status.Value ? "true" : "false")}");
            }

            if (filters.Count == 0)
            {
                return ("/odata/NewsArticles/Default.GetActive()", string.Empty);
            }

            return ("/odata/NewsArticlesFunctions/Search", string.Join("&", filters));
        }

        // Helper to build query-string filters for REST endpoints (Search)
        // 3. fetch category stats via /odata/NewsArticlesFunctions/ByCategory
        private async Task LoadCategoryStatsAsync()
        {
            try
            {
                var url = BuildODataFunctionUrl(CategoryReportEndpoint);
                var report = await _apiService.GetByIdAsync<CategoryReportModel>(url);

                CategoryStats = report?.CategoryStatistics ?? new List<CategoryStatisticModel>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading category statistics: {ex.Message}";
                CategoryStats = new List<CategoryStatisticModel>();
            }
        }

        // 4. fetch author stats via /odata/NewsArticles/ArticlesByAuthor
        private async Task LoadAuthorStatsAsync()
        {
            try
            {
                var url = BuildODataFunctionUrl(AuthorReportEndpoint);
                var report = await _apiService.GetByIdAsync<AuthorReportModel>(url);

                AuthorStats = report?.AuthorStatistics ?? new List<AuthorStatisticModel>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading author statistics: {ex.Message}";
                AuthorStats = new List<AuthorStatisticModel>();
            }
        }

        private string BuildODataFunctionUrl(string baseEndpoint)
        {
            var start = StartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
            var end = EndDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            var statusValue = Status.HasValue ? Status.Value.ToString().ToLowerInvariant() : "true";

            // Result: /odata/NewsArticles/Default.ArticlesByCategory(startDate='2026-01-01',endDate='2026-01-31',status=true)
            return $"{baseEndpoint}(startDate='{start}',endDate='{end}',status={statusValue})";
        }
    }
}