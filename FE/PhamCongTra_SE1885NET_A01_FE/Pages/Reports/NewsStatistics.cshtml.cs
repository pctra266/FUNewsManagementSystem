using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Reports
{
    [Authorize(Roles = "ADMIN")]
    public class NewsStatisticsModel : PageModel
    {
        private readonly INewsService _newsService;

        public NewsStatisticsModel(INewsService newsService)
        {
            _newsService = newsService;
        }

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.Today;

        // Sử dụng Model đã tách ra (NewsStatistic)
        public List<NewsStatistic> Statistics { get; set; } = new List<NewsStatistic>();

        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadStatisticsData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (StartDate > EndDate)
            {
                ErrorMessage = "Start date cannot be later than end date.";
                // Không return Page() ngay mà vẫn nên load lại data cũ hoặc data rỗng để view không bị lỗi
                Statistics = new List<NewsStatistic>();
                return Page();
            }

            await LoadStatisticsData();
            return Page();
        }

        private async Task LoadStatisticsData()
        {
            try
            {
                // Gọi Service thay vì HttpClient trực tiếp
                Statistics = await _newsService.GetNewsStatisticsAsync(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading statistics: " + ex.Message;
                Statistics = new List<NewsStatistic>();
            }
        }
    }
}
