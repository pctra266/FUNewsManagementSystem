using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;
        private readonly ITagService _tagService;

        public IndexModel(
            INewsArticleService newsService,
            ICategoryService categoryService,
            IAccountService accountService,
            ITagService tagService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _accountService = accountService;
            _tagService = tagService;
        }

        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
        public int TotalUsers { get; set; }

        public List<NewsArticle> RecentArticles { get; set; } = new();
        public List<CategoryWithCount> TopCategories { get; set; } = new();

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public List<NewsStatistic> Statistics { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDashboardData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDashboardData();
            // Load statistics for date range
            // Note: You need to add this method to INewsArticleService
            return Page();
        }

        private async Task LoadDashboardData()
        {
            // Load all data
            var allArticles = await _newsService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            var accounts = await _accountService.GetAllAsync();
            var tags = await _tagService.GetAllAsync();

            // Calculate statistics
            TotalArticles = allArticles.Count;
            ActiveArticles = allArticles.Count(a => a.NewsStatus == true);
            TotalCategories = categories.Count;
            TotalTags = tags.Count;
            TotalUsers = accounts.Count;

            // Get recent articles
            RecentArticles = allArticles
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .ToList();

            // Get top categories
            TopCategories = categories
                .OrderByDescending(c => c.ArticleCount)
                .Take(5)
                .ToList();
        }
    }

    public class NewsStatistic
    {
        public string DateString { get; set; }
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
    }
}