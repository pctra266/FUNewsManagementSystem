using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

namespace Presentation_RazorPage.Pages.Staff.Tags
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly HttpClient _httpClient;

        public IndexModel(IApiService apiService, HttpClient httpClient)
        {
            _apiService = apiService;
            _httpClient = httpClient;
        }

        public List<TagModel> Tags { get; set; } = new List<TagModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public int TotalTags { get; set; }
        public int UsedTags { get; set; }
        public int UnusedTags { get; set; }

        [BindProperty]
        public TagModel CreateTag { get; set; } = new TagModel();

        public async Task<IActionResult> OnGetAsync(string? searchTerm = null)
        {
            // Check if user is logged in and is staff
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole != "1") // Only Staff can manage tags
            {
                return RedirectToPage("/Access/Denied");
            }

            SearchTerm = searchTerm ?? string.Empty;
            //_apiService.SetAuthToken(token);

            try
            {
                // Get all tags with NewsArticles count
                string query = "/odata/Tags?$expand=NewsArticles($select=NewsArticleId)";
                var tagsResponse = await _apiService.GetAsync<TagWithArticlesModel>(query);

                var allTags = tagsResponse?.Select(t => new TagModel
                {
                    TagId = t.TagId,
                    TagName = t.TagName,
                    Note = t.Note,
                    ArticleCount = t.NewsArticles?.Count ?? 0
                }).ToList() ?? new List<TagModel>();

                // Apply search filter
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    allTags = allTags.Where(t =>
                        t.TagName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        t.Note?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
                    ).ToList();
                }

                Tags = allTags.OrderBy(t => t.TagName).ToList();

                // Calculate statistics
                TotalTags = allTags.Count;
                UsedTags = allTags.Count(t => t.ArticleCount > 0);
                UnusedTags = allTags.Count(t => t.ArticleCount == 0);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading tags: {ex.Message}";
                Tags = new List<TagModel>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                return RedirectToPage("/Access/Denied");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            //_apiService.SetAuthToken(token);

            try
            {
                var createData = new
                {
                    TagName = CreateTag.TagName,
                    Note = CreateTag.Note
                };

                var response = await _apiService.PostAsync<TagModel>("/odata/Tags", createData);
                if (response != null)
                {
                    TempData["SuccessMessage"] = "Tag created successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create tag. Tag name might already exist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating tag: {ex.Message}";
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int tagId, string tagName, string? note)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                return RedirectToPage("/Access/Denied");
            }

            //_apiService.SetAuthToken(token);

            try
            {
                var updateData = new
                {
                    TagName = tagName,
                    Note = note
                };

                var response = await _apiService.PutAsync<TagModel>("/odata/Tags", tagId, updateData);
                if (response != null)
                {
                    TempData["SuccessMessage"] = "Tag updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update tag. Tag name might already exist.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating tag: {ex.Message}";
            }

            return RedirectToPage("./Index", new { searchTerm = SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int tagId)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                return RedirectToPage("/Access/Denied");
            }

            //_apiService.SetAuthToken(token);

            try
            {
                var success = await _apiService.DeleteAsync("/odata/Tags", tagId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Tag deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot delete tag - it is being used by articles. Remove it from articles first.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting tag: {ex.Message}";
            }

            return RedirectToPage("./Index", new { searchTerm = SearchTerm });
        }
    }

    // Helper class to deserialize tags with articles
    public class TagWithArticlesModel
    {
        public int TagId { get; set; }
        public string? TagName { get; set; }
        public string? Note { get; set; }
        public List<ArticleIdModel>? NewsArticles { get; set; }
    }

    public class ArticleIdModel
    {
        public string NewsArticleId { get; set; } = string.Empty;
    }
}