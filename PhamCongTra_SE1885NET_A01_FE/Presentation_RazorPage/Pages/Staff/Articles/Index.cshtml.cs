using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.Staff.Articles
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<NewsArticleModel> MyArticles { get; set; } = new List<NewsArticleModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public short? CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool? StatusFilter { get; set; }

        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (string.IsNullOrEmpty(token) || (userRole != "1" && userRole != "Admin") || !userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Get user's articles
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles/GetByAuthor?authorId={userId}");
                var allMyArticles = articlesResponse ?? new List<NewsArticleModel>();

                // Apply filters
                MyArticles = allMyArticles;

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    MyArticles = MyArticles.Where(a => 
                        a.NewsTitle?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        a.NewsContent?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();
                }

                if (CategoryFilter.HasValue)
                {
                    MyArticles = MyArticles.Where(a => a.CategoryId == CategoryFilter).ToList();
                }

                if (StatusFilter.HasValue)
                {
                    MyArticles = MyArticles.Where(a => a.NewsStatus == StatusFilter).ToList();
                }

                // Calculate statistics
                TotalArticles = allMyArticles.Count;
                PublishedArticles = allMyArticles.Count(a => a.NewsStatus == true);
                DraftArticles = allMyArticles.Count(a => a.NewsStatus != true);

                // Get categories for filter dropdown
                await LoadCategoriesAsync();
            }
            catch (Exception)
            {
                MyArticles = new List<NewsArticleModel>();
                Categories = new List<CategoryModel>();
            }

            return Page();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = response ?? new List<CategoryModel>();
            }
            catch (Exception)
            {
                Categories = new List<CategoryModel>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            _apiService.SetAuthToken(token!);

            try
            {
                var success = await _apiService.DeleteAsync("/odata/NewsArticles", $"'{id}'");
                if (success)
                {
                    TempData["SuccessMessage"] = "Article deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete article.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the article.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string id, bool currentStatus)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            _apiService.SetAuthToken(token!);

            try
            {
                // Get the article first
                var article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'");
                if (article != null)
                {
                    article.NewsStatus = !currentStatus;
                    var result = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'", article);
                    
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = $"Article {(article.NewsStatus == true ? "published" : "unpublished")} successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update article status.";
                    }
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the article.";
            }

            return RedirectToPage();
        }
    }
}