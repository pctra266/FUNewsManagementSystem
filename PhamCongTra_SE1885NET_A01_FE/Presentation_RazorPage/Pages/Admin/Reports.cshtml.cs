using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

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

        private const string ExpandClause = "$expand=Category($select=CategoryName),CreatedBy($select=AccountName)";

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
                StartDate = new DateTime(2020, 1, 1); // Expand range to include sample data
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
                var filters = new List<string>();

                if (StartDate.HasValue)
                {
                    filters.Add($"CreatedDate ge {StartDate:yyyy-MM-dd}T00:00:00Z");
                }

                if (EndDate.HasValue)
                {
                    filters.Add($"CreatedDate le {EndDate:yyyy-MM-dd}T23:59:59Z");
                }

                var filterQuery = filters.Any() ? $"$filter={string.Join(" and ", filters)}" : "";
                var searchUrl = $"/odata/NewsArticles?{filterQuery}{(string.IsNullOrEmpty(filterQuery) ? "" : "&")}{ExpandClause}";
                
                var searchResponse = await _apiService.GetAsync<NewsArticleModel>(searchUrl);

                ArticleDetails = (searchResponse ?? new List<NewsArticleModel>());
                foreach (var article in ArticleDetails)
                {
                    article.HydrateMetadata();
                }

                ArticleDetails = SortOrder == "asc"
                    ? ArticleDetails.OrderBy(a => a.CreatedDate).ToList()
                    : ArticleDetails.OrderByDescending(a => a.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading article details: {ex.Message}";
                ArticleDetails = new List<NewsArticleModel>();
            }
        }

        private async Task LoadCategoryStatsAsync()
        {
            try
            {
                var queryParams = new List<string>();
                if (StartDate.HasValue) queryParams.Add($"startDate={StartDate.Value:yyyy-MM-dd}");
                if (EndDate.HasValue) queryParams.Add($"endDate={EndDate.Value:yyyy-MM-dd}");
                if (Status.HasValue) queryParams.Add($"status={Status.Value.ToString().ToLower()}");

                var query = string.Join(",", queryParams);
                var url = $"/odata/Reports/Default.ArticlesByCategory({query})";

                var categoryResponse = await _apiService.GetByIdAsync<CategoryReportModel>(url);

                if (categoryResponse != null)
                {
                    CategoryStats = categoryResponse.CategoryStatistics ?? new List<CategoryStatisticModel>();
                }
                else
                {
                    CategoryStats = new List<CategoryStatisticModel>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading category statistics: {ex.Message}";
                CategoryStats = new List<CategoryStatisticModel>();
            }
        }

        private async Task LoadAuthorStatsAsync()
        {
            try
            {
                var queryParams = new List<string>();
                if (StartDate.HasValue) queryParams.Add($"startDate={StartDate.Value:yyyy-MM-dd}");
                if (EndDate.HasValue) queryParams.Add($"endDate={EndDate.Value:yyyy-MM-dd}");
                if (Status.HasValue) queryParams.Add($"status={Status.Value.ToString().ToLower()}");

                var query = string.Join(",", queryParams);
                var url = $"/odata/Reports/Default.ArticlesByAuthor({query})";

                var authorResponse = await _apiService.GetByIdAsync<AuthorReportModel>(url);

                if (authorResponse != null)
                {
                    AuthorStats = authorResponse.AuthorStatistics ?? new List<AuthorStatisticModel>();
                }
                else
                {
                    AuthorStats = new List<AuthorStatisticModel>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading author statistics: {ex.Message}";
                AuthorStats = new List<AuthorStatisticModel>();
            }
        }
    }
}