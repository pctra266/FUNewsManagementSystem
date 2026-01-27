using BusinessLogic.Services;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleService _newsService;
        private readonly ICategoryService _categoryService;

        public IndexModel(
            INewsArticleService newsService,
            ICategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        public List<NewsArticle> NewsArticles { get; set; } = new();
        public List<CategoryWithCount> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? FilterCategory { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterStatus { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            // Load categories for filter dropdown
            Categories = await _categoryService.GetAllAsync();

            // Search with filters
            var allArticles = await _newsService.SearchAsync(
                SearchKeyword,
                FilterCategory,
                FilterStatus);

            // Pagination
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(allArticles.Count / (double)PageSize);

            NewsArticles = allArticles
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            try
            {
                await _newsService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Article deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting article: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDuplicateAsync(string id)
        {
            try
            {
                var duplicate = await _newsService.DuplicateAsync(id);
                TempData["SuccessMessage"] = $"Article duplicated successfully! New ID: {duplicate?.NewsArticleId}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error duplicating article: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}