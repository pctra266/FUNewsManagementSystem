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
        public string ArticleId { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Staff/Articles/Index");
            }

            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);
            ArticleId = id;

            try
            {
                // Load article
                var article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'");

                if (article == null)
                {
                    TempData["ErrorMessage"] = "Article not found.";
                    return RedirectToPage("/Staff/Articles/Index");
                }

                // Populate view model
                Article = new NewsArticleEditViewModel
                {
                    NewsTitle = article.NewsTitle ?? "",
                    Headline = article.Headline ?? "",
                    NewsContent = article.NewsContent,
                    NewsSource = article.NewsSource,
                    CategoryId = article.CategoryId ?? 0,
                    NewsStatus = article.NewsStatus ?? true,
                    SelectedTagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>()
                };

                // Load categories and tags
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

                var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
                Tags = tagsResponse ?? new List<TagModel>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading article: {ex.Message}";
                return RedirectToPage("/Staff/Articles/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Staff/Articles/Index");
            }

            ArticleId = id;

            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                // Update article with tags
                var updateData = new
                {
                    NewsTitle = Article.NewsTitle,
                    Headline = Article.Headline,
                    NewsContent = Article.NewsContent,
                    NewsSource = Article.NewsSource,
                    CategoryId = Article.CategoryId,
                    NewsStatus = Article.NewsStatus,
                    TagIds = Article.SelectedTagIds
                };

                var result = await _apiService.PutAsync<object>("/odata/NewsArticles", $"'{id}'", updateData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article updated successfully! UpdatedById and ModifiedDate have been set automatically.";
                    return RedirectToPage("/Staff/Articles/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update article. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
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

                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

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

        public bool NewsStatus { get; set; } = true;

        public List<int> SelectedTagIds { get; set; } = new List<int>();
    }
}