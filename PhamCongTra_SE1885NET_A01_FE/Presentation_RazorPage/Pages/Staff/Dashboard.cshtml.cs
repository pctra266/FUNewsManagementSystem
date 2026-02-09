using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

namespace Presentation_RazorPage.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        private readonly IApiService _apiService;

        public DashboardModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public DashboardStatisticsModel DashboardStats { get; set; } = new DashboardStatisticsModel();
        public List<NewsArticleModel> TrendingArticles { get; set; } = new List<NewsArticleModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole == "1")
            {
                var stats = await _apiService.GetByIdAsync<DashboardStatisticsModel>("/odata/Reports/Default.Dashboard()");
                if (stats != null)
                {
                    DashboardStats = stats;
                }

                var trending = await _apiService.GetAsync<NewsArticleModel>("/odata/Reports/Default.Trending(top=5)");
                if (trending != null)
                {
                    TrendingArticles = trending;
                }

                return Page();
            }

            return RedirectToPage("/News/Index");
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            var fileData = await _apiService.DownloadFileAsync("/odata/Reports/Default.Export()");
            if (fileData == null || fileData.Length == 0)
            {
                TempData["ErrorMessage"] = "Failed to download report.";
                return RedirectToPage();
            }

            var fileName = $"ArticleReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}