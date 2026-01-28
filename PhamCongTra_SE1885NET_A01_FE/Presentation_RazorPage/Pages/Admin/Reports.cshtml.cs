using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

namespace Presentation_RazorPage.Pages.Admin
{
    public class ReportsModel : PageModel
    {
        private readonly IApiService _apiService;

        public ReportsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<CategoryStatisticModel> CategoryStats { get; set; } = new List<CategoryStatisticModel>();
        public List<AuthorStatisticModel> AuthorStats { get; set; } = new List<AuthorStatisticModel>();

        public int TotalArticles { get; set; }
        public int TotalActiveArticles { get; set; }
        public int TotalInactiveArticles { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string GroupBy { get; set; } = "category";

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

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
                TotalArticles = 0;
                TotalActiveArticles = 0;
                TotalInactiveArticles = 0;
            }

            return Page();
        }

        private async Task LoadReportDataAsync()
        {
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

        private async Task LoadCategoryStatsAsync()
        {
            try
            {
                var query = "";
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    query = $"?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}";
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