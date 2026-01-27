using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace Presentation_RazorPage.Pages.Staff.Articles
{
    public class EditModel : PageModel
    {
        private readonly IApiService _apiService;

        public EditModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public NewsArticleEditViewModel Article { get; set; } = new NewsArticleEditViewModel();

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (string.IsNullOrEmpty(token) || (userRole != "1" && userRole != "Admin") || !userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Get the article
                var article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'");
                if (article == null)
                {
                    return NotFound();
                }

                // Check if user owns this article (staff can only edit their own articles)
                if (userRole == "1" && article.CreatedById != userId)
                {
                    return Forbid();
                }

                // Map to edit model
                Article = new NewsArticleEditViewModel
                {
                    NewsArticleId = article.NewsArticleId,
                    NewsTitle = article.NewsTitle ?? string.Empty,
                    Headline = article.Headline,
                    NewsContent = article.NewsContent,
                    NewsSource = article.NewsSource,
                    CategoryId = article.CategoryId ?? 0,
                    NewsStatus = article.NewsStatus,
                    SelectedTagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>()
                };

                // Load categories and tags
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();

                var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
                Tags = tagsResponse ?? new List<TagModel>();
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                // Create updated article object
                var updatedArticle = new NewsArticleModel
                {
                    NewsArticleId = Article.NewsArticleId,
                    NewsTitle = Article.NewsTitle,
                    Headline = Article.Headline,
                    NewsContent = Article.NewsContent,
                    NewsSource = Article.NewsSource,
                    CategoryId = Article.CategoryId,
                    NewsStatus = Article.NewsStatus
                };

                var result = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'", updatedArticle);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article updated successfully!";
                    return RedirectToPage("/Staff/Articles/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update article. Please try again.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the article.");
            }

            await LoadDropdownDataAsync();
            return Page();
        }

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();

                var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
                Tags = tagsResponse ?? new List<TagModel>();
            }
            catch
            {
                Categories = new List<CategoryModel>();
                Tags = new List<TagModel>();
            }
        }
    }

    public class NewsArticleEditViewModel
    {
        public string NewsArticleId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "News title is required")]
        [StringLength(400, ErrorMessage = "News title cannot exceed 400 characters")]
        public string NewsTitle { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        public string Headline { get; set; } = string.Empty;
        
        [StringLength(4000, ErrorMessage = "News content cannot exceed 4000 characters")]
        public string? NewsContent { get; set; }
        
        [StringLength(400, ErrorMessage = "News source cannot exceed 400 characters")]
        public string? NewsSource { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public short CategoryId { get; set; }
        
        public bool? NewsStatus { get; set; }
        
        public List<int> SelectedTagIds { get; set; } = new List<int>();
    }
}