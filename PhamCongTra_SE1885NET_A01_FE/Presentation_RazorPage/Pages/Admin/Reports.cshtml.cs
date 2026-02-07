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
                StartDate = DateTime.Today.AddDays(-30);
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
                StartDate = DateTime.Today.AddDays(-30);
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
                var queryParts = new List<string>();

                if (StartDate.HasValue)
                    queryParts.Add($"startDate={StartDate:yyyy-MM-dd}");

                if (EndDate.HasValue)
                    queryParts.Add($"endDate={EndDate:yyyy-MM-dd}");

                var query = string.Join("&", queryParts);
                var searchUrl = $"/odata/NewsArticlesFunctions/Search?{query}{(string.IsNullOrEmpty(query) ? string.Empty : "&")}{ExpandClause}";
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
                var query = "";
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    query = $"?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}";
                }
                else if (StartDate.HasValue || EndDate.HasValue)
                {
                    query = "?";
                    if (StartDate.HasValue) query += $"startDate={StartDate:yyyy-MM-dd}";
                    if (EndDate.HasValue) query += $"endDate={EndDate:yyyy-MM-dd}";
                }

                if (Status.HasValue)
                {
                    query += string.IsNullOrEmpty(query) ? "?" : "&";
                    query += $"status={Status.Value.ToString().ToLower()}";
                }

                var categoryResponse = await _apiService.GetByIdAsync<CategoryReportModel>($"/api/Reports/ArticlesByCategory{query}");

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
                var query = "";
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    query = $"?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}";
                }
                else if (StartDate.HasValue || EndDate.HasValue)
                {
                    query = "?";
                    if (StartDate.HasValue) query += $"startDate={StartDate:yyyy-MM-dd}";
                    if (EndDate.HasValue) query += $"endDate={EndDate:yyyy-MM-dd}";
                }

                if (Status.HasValue)
                {
                    query += string.IsNullOrEmpty(query) ? "?" : "&";
                    query += $"status={Status.Value.ToString().ToLower()}";
                }

                var authorResponse = await _apiService.GetByIdAsync<AuthorReportModel>($"/api/Reports/ArticlesByAuthor{query}");

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