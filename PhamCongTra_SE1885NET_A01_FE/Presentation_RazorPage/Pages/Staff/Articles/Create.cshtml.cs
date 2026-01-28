using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace Presentation_RazorPage.Pages.Staff.Articles
{
    public class CreateModel : PageModel
    {
        private readonly IApiService _apiService;

        public CreateModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public NewsArticleCreateViewModel Article { get; set; } = new NewsArticleCreateViewModel();

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || (userRole != "1" && userRole != "Admin"))
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Load categories and tags
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();

                var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
                Tags = tagsResponse ?? new List<TagModel>();
            }
            catch (Exception)
            {
                Categories = new List<CategoryModel>();
                Tags = new List<TagModel>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

                // Create article with tags
                var createData = new
                {
                    NewsTitle = Article.NewsTitle,
                    Headline = Article.Headline,
                    NewsContent = Article.NewsContent,
                    NewsSource = Article.NewsSource,
                    CategoryId = Article.CategoryId,
                    NewsStatus = Article.NewsStatus,
                    TagIds = Article.SelectedTagIds
                };

                var result = await _apiService.PostAsync<object>("/odata/NewsArticles", createData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article created successfully! CreatedById has been set automatically.";
                    return RedirectToPage("/Staff/Articles/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create article. Please try again.");
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

    public class NewsArticleCreateViewModel
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