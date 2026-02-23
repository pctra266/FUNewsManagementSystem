using BusinessLogic.Services;
using System.IO;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation_RazorPage.Pages.Staff.NewsArticles
{
    public class EditModel : PageModel
    {
        private readonly IApiService _apiService;

        public EditModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<CategoryModel> Categories { get; set; } = new();
        public List<TagModel> Tags { get; set; } = new();

        [BindProperty]
        public NewsArticleEditInput EditArticle { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            var authResult = EnsureAuthorized();
            if (authResult != null)
            {
                return authResult;
            }

            Id ??= id;

            if (string.IsNullOrWhiteSpace(Id))
            {
                TempData["ErrorMessage"] = "Article ID is required.";
                return RedirectToSafeReturnUrl();
            }

            await PopulateLookupsAsync();

            var article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'", "?$expand=Category,Tags,NewsArticleImages");
            if (article == null)
            {
                TempData["ErrorMessage"] = "Article not found.";
                return RedirectToSafeReturnUrl();
            }

            EditArticle = new NewsArticleEditInput
            {
                NewsArticleId = article.NewsArticleId,
                NewsTitle = article.NewsTitle ?? string.Empty,
                Headline = article.Headline,
                NewsContent = article.NewsContent,
                NewsSource = article.NewsSource,
                CategoryId = article.CategoryId ?? 0,
                NewsStatus = article.NewsStatus ?? false,
                SelectedTagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>(),
                NewsArticleImages = article.NewsArticleImages?.ToList() ?? new List<NewsArticleImageModel>()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<IFormFile> imageFiles)
        {
            // Server-side image validation (fallback when JavaScript is disabled)
            if (imageFiles != null && imageFiles.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                const long maxFileSize = 5 * 1024 * 1024; // 5 MB

                foreach (var file in imageFiles.Where(f => f.Length > 0))
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                        ModelState.AddModelError(string.Empty,
                            $"'{file.FileName}': Invalid file type. Only JPEG, PNG and GIF images are allowed.");
                    else if (file.Length > maxFileSize)
                        ModelState.AddModelError(string.Empty,
                            $"'{file.FileName}': File size exceeds the 5 MB limit.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                // Reload existing images so the form can display them correctly
                if (!string.IsNullOrWhiteSpace(EditArticle.NewsArticleId))
                {
                    var article = await _apiService.GetByIdAsync<NewsArticleModel>(
                        "/odata/NewsArticles", $"'{EditArticle.NewsArticleId}'", "?$expand=NewsArticleImages");
                    EditArticle.NewsArticleImages = article?.NewsArticleImages?.ToList()
                        ?? new List<NewsArticleImageModel>();
                }
                return Page();
            }

            try
            {
                // Update Article
                var payload = new
                {
                    NewsTitle = EditArticle.NewsTitle,
                    Headline = EditArticle.Headline,
                    NewsContent = EditArticle.NewsContent,
                    NewsSource = EditArticle.NewsSource,
                    CategoryId = EditArticle.CategoryId,
                    NewsStatus = EditArticle.NewsStatus,
                    TagIds = EditArticle.SelectedTagIds
                };

                var updatedArticle = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{EditArticle.NewsArticleId}'", payload);

                if (updatedArticle != null)
                {
                     // Upload New Images
                    if (imageFiles != null && imageFiles.Any())
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0)
                            {
                                var imageUrl = await _apiService.UploadImageAsync("/api/NewsArticles/upload-image", file.OpenReadStream(), file.FileName);
                                
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    var imagePayload = new
                                    {
                                        NewsArticleId = EditArticle.NewsArticleId,
                                        ImageUrl = imageUrl,
                                        Caption = file.FileName
                                    };
                                    await _apiService.PostAsync<object>($"/api/NewsArticleImages/article/{updatedArticle.NewsArticleId}", imagePayload);
                                }
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Article updated successfully!";
                    return RedirectToSafeReturnUrl();
                }

                ModelState.AddModelError(string.Empty, "Failed to update article.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            await PopulateLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int imageId, string articleId)
        {
             try
             {
                 var success = await _apiService.DeleteAsync($"/api/NewsArticleImages/{imageId}");
                 if (success)
                 {
                     TempData["SuccessMessage"] = "Image deleted successfully!";
                 }
                 else
                 {
                     TempData["ErrorMessage"] = "Failed to delete image.";
                 }
             }
             catch(Exception ex)
             {
                 TempData["ErrorMessage"] = $"Error deleting image: {ex.Message}";
             }

             return RedirectToPage(new { id = articleId });
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
                EditArticle.NewsSource = imageUrl;
            }
        }
    }
}