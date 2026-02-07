
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
            // Check if user is logged in
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // According to project requirements, Staff can use this dashboard now
            if (userRole == "1") // Staff
            {
                //_apiService.SetAuthToken(token);
                // Fetch stats
                // Fetch stats (Using GetByIdAsync as it returns single object)
                var stats = await _apiService.GetByIdAsync<DashboardStatisticsModel>("/api/Reports/Dashboard");
                if (stats != null) DashboardStats = stats;
                                 


                // Fetch Trending
                var trending = await _apiService.GetAsync<NewsArticleModel>("/api/Reports/Trending?top=5");
                if (trending != null) TrendingArticles = trending;

                return Page();
            }
            else
            {
                // Lecturer can only read and search articles
                return RedirectToPage("/News/Index");
            }
        }
        
        public async Task<IActionResult> OnGetExportAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");
            
            //_apiService.SetAuthToken(token);
            var fileData = await _apiService.DownloadFileAsync("/api/Reports/Export");
            
            if (fileData == null)
            {
                TempData["ErrorMessage"] = "Failed to download report.";
                return RedirectToPage();
            }

            var fileName = $"ArticleReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}