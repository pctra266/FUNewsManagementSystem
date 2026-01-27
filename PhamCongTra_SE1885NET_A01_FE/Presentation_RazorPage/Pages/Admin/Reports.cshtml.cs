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

        public DashboardStatisticsModel? DashboardStats { get; set; }
        public List<CategoryStatisticModel> CategoryStats { get; set; } = new List<CategoryStatisticModel>();
        public List<AuthorStatisticModel> AuthorStats { get; set; } = new List<AuthorStatisticModel>();
        public List<MonthlyStatistic> MonthlyStats { get; set; } = new List<MonthlyStatistic>();

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = "dashboard";

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            // Set default date range if not provided (last 30 days)
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
                // Handle errors gracefully
                TempData["ErrorMessage"] = $"Error loading report data: {ex.Message}";
                DashboardStats = new DashboardStatisticsModel();
                CategoryStats = new List<CategoryStatisticModel>();
                AuthorStats = new List<AuthorStatisticModel>();
                MonthlyStats = new List<MonthlyStatistic>();
            }

            return Page();
        }

        private async Task LoadReportDataAsync()
        {
            switch (ReportType.ToLower())
            {
                case "dashboard":
                    await LoadDashboardStatsAsync();
                    break;
                case "category":
                    await LoadCategoryStatsAsync();
                    break;
                case "author":
                    await LoadAuthorStatsAsync();
                    break;
                case "monthly":
                    await LoadMonthlyStatsAsync();
                    break;
                default:
                    await LoadDashboardStatsAsync();
                    break;
            }
        }

        private async Task LoadDashboardStatsAsync()
        {
            try
            {
                var dashboardResponse = await _apiService.GetByIdAsync<DashboardStatisticsModel>("/api/Reports/Dashboard");
                if (dashboardResponse != null)
                {
                    DashboardStats = dashboardResponse;
                }
                else
                {
                    // Create default stats if no data
                    DashboardStats = new DashboardStatisticsModel
                    {
                        TotalArticles = 0,
                        ActiveArticles = 0,
                        InactiveArticles = 0,
                        TotalCategories = 0,
                        TotalAccounts = 0,
                        TotalTags = 0,
                        StaffAccounts = 0,
                        LecturerAccounts = 0,
                        ActiveCategories = 0
                    };
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard statistics: {ex.Message}";
                DashboardStats = new DashboardStatisticsModel();
            }
        }

        private async Task LoadCategoryStatsAsync()
        {
            try
            {
                // Create query string for date range
                var query = "";
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    query = $"?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}";
                }

                var categoryResponse = await _apiService.GetByIdAsync<CategoryReportModel>($"/api/Reports/ArticlesByCategory{query}");
                
                if (categoryResponse != null)
                {
                    var report = categoryResponse;
                    CategoryStats = report.CategoryStatistics ?? new List<CategoryStatisticModel>();
                    
                    // Calculate percentage for each category
                    var totalArticles = CategoryStats.Sum(c => c.TotalArticles);
                    foreach (var stat in CategoryStats)
                    {
                        stat.Percentage = totalArticles > 0 ? Math.Round((double)stat.TotalArticles / totalArticles * 100.0, 1) : 0;
                    }
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
                    var report = authorResponse;
                    AuthorStats = report.AuthorStatistics ?? new List<AuthorStatisticModel>();
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

        private async Task LoadMonthlyStatsAsync()
        {
            try
            {
                var year = EndDate?.Year ?? DateTime.Now.Year;
                var monthlyResponse = await _apiService.GetByIdAsync<MonthlyStatsModel>($"/api/Reports/MonthlyStats?year={year}");
                
                if (monthlyResponse != null)
                {
                    MonthlyStats = monthlyResponse.MonthlyStatistics;
                }
                else
                {
                    MonthlyStats = new List<MonthlyStatistic>();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading monthly statistics: {ex.Message}";
                MonthlyStats = new List<MonthlyStatistic>();
            }
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            // Set default date range if not provided
            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                EndDate = DateTime.Today;
                StartDate = DateTime.Today.AddDays(-30);
            }

            try
            {
                // Load current report data
                await LoadReportDataAsync();

                byte[] excelData;
                string fileName;

                switch (ReportType.ToLower())
                {
                    case "dashboard":
                        excelData = _excelExportService.ExportDashboardReport(DashboardStats ?? new DashboardStatisticsModel(), StartDate, EndDate);
                        fileName = $"Dashboard_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "category":
                        excelData = _excelExportService.ExportCategoryReport(CategoryStats, StartDate, EndDate);
                        fileName = $"Category_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "author":
                        excelData = _excelExportService.ExportAuthorReport(AuthorStats, StartDate, EndDate);
                        fileName = $"Author_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "monthly":
                        var year = EndDate?.Year ?? DateTime.Now.Year;
                        excelData = _excelExportService.ExportMonthlyReport(MonthlyStats, year);
                        fileName = $"Monthly_Report_{year}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    default:
                        excelData = _excelExportService.ExportDashboardReport(DashboardStats ?? new DashboardStatisticsModel(), StartDate, EndDate);
                        fileName = $"Dashboard_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;
                }

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating Excel export: {ex.Message}";
                return RedirectToPage(new { ReportType, StartDate, EndDate });
            }
        }
    }
}