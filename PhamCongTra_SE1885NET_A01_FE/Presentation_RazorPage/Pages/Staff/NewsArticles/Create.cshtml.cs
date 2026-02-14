using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation_RazorPage.Pages.Staff.NewsArticles
{
    public class CreateModel : PageModel
    {
        private readonly IApiService _apiService;

        public CreateModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<CategoryModel> Categories { get; set; } = new();
        public List<TagModel> Tags { get; set; } = new();

        [BindProperty]
        public NewsArticleCreateInput CreateArticle { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = EnsureAuthorized();
            if (authResult != null)
            {
                return authResult;
            }

            await PopulateLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            var authResult = EnsureAuthorized();
            if (authResult != null)
            {
                return authResult;
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return Page();
            }

            try
            {
                await UploadImageIfNeededAsync(imageFile);

                var payload = new
                {
                    NewsTitle = CreateArticle.NewsTitle,
                    Headline = CreateArticle.Headline,
                    NewsContent = CreateArticle.NewsContent,
                    NewsSource = CreateArticle.NewsSource,
                    CategoryId = CreateArticle.CategoryId,
                    NewsStatus = CreateArticle.NewsStatus,
                    TagIds = CreateArticle.SelectedTagIds
                };

                var result = await _apiService.PostAsync<object>("/odata/NewsArticles", payload);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article created successfully!";
                    return RedirectToSafeReturnUrl();
                }

                ModelState.AddModelError(string.Empty, "Failed to create article. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            await PopulateLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSuggestTagsAsync([FromBody] TagSuggestionRequest request)
        {
            var authResult = EnsureAuthorized();
            if (authResult is RedirectToPageResult redirect)
            {
                return redirect;
            }

            try
            {
                var suggestions = await _apiService.PostAsync<Dictionary<string, double>>("/api/AI/suggest-tags", request);
                return new JsonResult(suggestions ?? new Dictionary<string, double>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public string GetSafeReturnUrl()
        {
            var fallback = Url.Page("/Staff/NewsArticles/Index") ?? "/Staff/NewsArticles";
            return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
                ? ReturnUrl!
                : fallback;
        }

        private IActionResult RedirectToSafeReturnUrl()
        {
            return Redirect(GetSafeReturnUrl());
        }

        private IActionResult? EnsureAuthorized()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied. Only Staff can manage news articles.";
                return RedirectToPage("/Index");
            }

            return null;
        }

        private async Task PopulateLookupsAsync()
        {
            var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
            Categories = categoriesResponse?.Where(c => c.IsActive == true)
                       .OrderBy(c => c.CategoryName)
                       .ToList() ?? new List<CategoryModel>();

            var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
            Tags = tagsResponse?.OrderBy(t => t.TagName).ToList() ?? new List<TagModel>();
        }

        private async Task UploadImageIfNeededAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return;
            }

            using var stream = imageFile.OpenReadStream();
            var imageUrl = await _apiService.UploadImageAsync("/api/NewsArticles/upload-image", stream, imageFile.FileName);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                CreateArticle.NewsSource = imageUrl;
            }
        }
    }
}