using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

namespace Presentation_RazorPage.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IApiService _apiService;

        public DashboardModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public DashboardStatisticsModel DashboardData { get; set; } = new DashboardStatisticsModel();
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in and is admin
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole != "Admin")
            {
                return RedirectToPage("/Staff/Dashboard");
            }

            UserName = HttpContext.Session.GetString("UserName") ?? "Admin";
            UserRole = userRole ?? "Admin";

            // Set auth token
            _apiService.SetAuthToken(token);

            try
            {
                // Get dashboard statistics from the API
                var dashboardResponse = await _apiService.GetByIdAsync<DashboardStatisticsModel>("/api/Reports/Dashboard");
                
                if (dashboardResponse != null)
                {
                    DashboardData = dashboardResponse;
                }
                else
                {
                    // If API call fails or returns no data, initialize with empty data
                    DashboardData = new DashboardStatisticsModel();
                    HasError = true;
                    ErrorMessage = "Unable to load dashboard statistics. Please try refreshing the page.";
                }
            }
            catch (Exception ex)
            {
                // Handle error gracefully, dashboard will show without data
                DashboardData = new DashboardStatisticsModel();
                HasError = true;
                ErrorMessage = $"Error loading dashboard data: {ex.Message}";
            }

            return Page();
        }

        public async Task<JsonResult> OnGetRefreshDataAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            
            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { error = "Not authenticated" }) { StatusCode = 401 };
            }

            _apiService.SetAuthToken(token);

            try
            {
                var dashboardResponse = await _apiService.GetByIdAsync<DashboardStatisticsModel>("/api/Reports/Dashboard");
                
                if (dashboardResponse != null )
                {
                    return new JsonResult(new { success = true, data = dashboardResponse });
                }
                else
                {
                    return new JsonResult(new { success = false, error = "No data available" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}