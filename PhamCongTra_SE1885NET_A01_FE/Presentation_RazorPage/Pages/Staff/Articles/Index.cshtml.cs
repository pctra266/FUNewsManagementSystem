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

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied. Only Staff can manage articles.";
                return RedirectToPage("/Index");
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Load categories
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

                // Build OData query for current user's articles
                var query = BuildODataQuery(userId!.Value);
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles{query}");

                if (articlesResponse != null)
                {
                    MyArticles = articlesResponse.ToList();

                    // Calculate statistics
                    TotalArticles = MyArticles.Count;
                    PublishedArticles = MyArticles.Count(a => a.NewsStatus == true);
                    DraftArticles = MyArticles.Count(a => a.NewsStatus != true);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading articles: {ex.Message}";
            }

            return Page();
        }

        private string BuildODataQuery(int userId)
        {
            var filters = new List<string>();
            var queryParts = new List<string>();

            // Filter by current user (CreatedById)
            filters.Add($"CreatedById eq {userId}");

            // Search by title, content, or headline
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filters.Add($"(contains(tolower(NewsTitle), '{SearchTerm.ToLower()}') or contains(tolower(NewsContent), '{SearchTerm.ToLower()}') or contains(tolower(Headline), '{SearchTerm.ToLower()}'))");
            }

            // Filter by category
            if (CategoryFilter.HasValue)
            {
                filters.Add($"CategoryId eq {CategoryFilter}");
            }

            // Filter by status
            if (StatusFilter.HasValue)
            {
                filters.Add($"NewsStatus eq {StatusFilter.ToString().ToLower()}");
            }

            // Filter by date range
            if (StartDate.HasValue)
            {
                filters.Add($"CreatedDate ge {StartDate:yyyy-MM-dd}T00:00:00Z");
            }

            if (EndDate.HasValue)
            {
                filters.Add($"CreatedDate le {EndDate:yyyy-MM-dd}T23:59:59Z");
            }

            // Expand related entities
            queryParts.Add("$expand=Category,CreatedBy,Tags");

            // Add filters
            if (filters.Any())
            {
                queryParts.Add($"$filter={string.Join(" and ", filters)}");
            }

            // Sort by CreatedDate descending by default
            queryParts.Add($"$orderby=CreatedDate {SortOrder}");

            return queryParts.Any() ? "?" + string.Join("&", queryParts) : "";
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToPage();
            }

            _apiService.SetAuthToken(token);

            try
            {
                var success = await _apiService.DeleteAsync("/odata/NewsArticles", $"'{id}'");
                if (success)
                {
                    TempData["SuccessMessage"] = "Article deleted successfully! Related tags have been removed.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete article.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                categoryFilter = CategoryFilter,
                statusFilter = StatusFilter,
                startDate = StartDate?.ToString("yyyy-MM-dd"),
                endDate = EndDate?.ToString("yyyy-MM-dd"),
                sortOrder = SortOrder
            });
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string id, bool currentStatus)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToPage();
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Get current article with tags using OData expand
                var articlesResponse = await _apiService.GetByIdAsync<NewsArticleModel>($"/odata/NewsArticles", $"'{id}'", "?$expand=Tags");
                var article = articlesResponse;

                if (article == null)
                {
                    TempData["ErrorMessage"] = "Article not found.";
                    return RedirectToPage();
                }

                // Toggle status while preserving all other data including tags
                var updateData = new
                {
                    NewsTitle = article.NewsTitle,
                    Headline = article.Headline,
                    NewsContent = article.NewsContent,
                    NewsSource = article.NewsSource,
                    CategoryId = article.CategoryId,
                    NewsStatus = !currentStatus,
                    TagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>()
                };

                var result = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'", updateData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = currentStatus
                        ? "Article unpublished successfully!"
                        : "Article published successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update article status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                categoryFilter = CategoryFilter,
                statusFilter = StatusFilter,
                startDate = StartDate?.ToString("yyyy-MM-dd"),
                endDate = EndDate?.ToString("yyyy-MM-dd"),
                sortOrder = SortOrder
            });
        }

        public async Task<IActionResult> OnPostDuplicateAsync(string id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToPage();
            }

            _apiService.SetAuthToken(token);

            try
            {
                var duplicateData = new { ArticleId = id };
                var result = await _apiService.PostAsync<object>("/odata/NewsArticlesFunctions/Duplicate", duplicateData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article duplicated successfully! The copy has been created as a draft.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to duplicate article.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                categoryFilter = CategoryFilter,
                statusFilter = StatusFilter,
                startDate = StartDate?.ToString("yyyy-MM-dd"),
                endDate = EndDate?.ToString("yyyy-MM-dd"),
                sortOrder = SortOrder
            });
        }
    }
}