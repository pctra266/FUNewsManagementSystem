using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;

namespace Presentation_RazorPage.Pages.Staff.NewsArticles
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<NewsArticleModel> NewsArticles { get; set; } = new List<NewsArticleModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AuthorFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "CreatedDate";

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int DraftArticles { get; set; }

        public async Task<IActionResult> OnGetAsync()
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

            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 50) PageSize = 50;

            _apiService.SetAuthToken(token);

            try
            {
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

                var query = BuildODataQuery();
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles{query}");

                if (articlesResponse != null)
                {
                    var allFilteredArticles = articlesResponse.ToList();

                    TotalArticles = allFilteredArticles.Count;
                    ActiveArticles = allFilteredArticles.Count(a => a.NewsStatus == true);
                    DraftArticles = allFilteredArticles.Count(a => a.NewsStatus != true);

                    var totalPages = (int)Math.Ceiling((double)TotalArticles / PageSize);
                    if (CurrentPage > totalPages && totalPages > 0)
                    {
                        CurrentPage = totalPages;
                    }

                    NewsArticles = allFilteredArticles
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();

                    Pagination = new PaginationInfo
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = totalPages,
                        TotalItems = TotalArticles,
                        PageSize = PageSize
                    };
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading articles: {ex.Message}";
            }

            return Page();
        }

        private string BuildODataQuery()
        {
            var filters = new List<string>();
            var queryParts = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filters.Add($"(contains(tolower(NewsTitle), '{SearchTerm.ToLower()}') or contains(tolower(NewsContent), '{SearchTerm.ToLower()}') or contains(tolower(Headline), '{SearchTerm.ToLower()}'))");
            }

            if (CategoryFilter.HasValue)
            {
                filters.Add($"CategoryId eq {CategoryFilter}");
            }

            if (StatusFilter.HasValue)
            {
                filters.Add($"NewsStatus eq {StatusFilter.ToString().ToLower()}");
            }

            if (!string.IsNullOrEmpty(AuthorFilter))
            {
                filters.Add($"contains(tolower(CreatedBy/AccountName), '{AuthorFilter.ToLower()}')");
            }

            if (StartDate.HasValue)
            {
                filters.Add($"CreatedDate ge {StartDate:yyyy-MM-dd}T00:00:00Z");
            }

            if (EndDate.HasValue)
            {
                filters.Add($"CreatedDate le {EndDate:yyyy-MM-dd}T23:59:59Z");
            }

            queryParts.Add("$expand=Category,CreatedBy,Tags");

            if (filters.Any())
            {
                queryParts.Add($"$filter={string.Join(" and ", filters)}");
            }

            var orderBy = SortBy switch
            {
                "NewsTitle" => "NewsTitle",
                "CategoryName" => "Category/CategoryName",
                "AuthorName" => "CreatedBy/AccountName",
                "NewsStatus" => "NewsStatus",
                _ => "CreatedDate"
            };

            queryParts.Add($"$orderby={orderBy} {SortOrder}");

            return queryParts.Any() ? "?" + string.Join("&", queryParts) : "";
        }

        public async Task<IActionResult> OnPostDeleteAsync(string articleId)
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
                var success = await _apiService.DeleteAsync("/odata/NewsArticles", $"'{articleId}'");
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

            return RedirectToPage(new { searchTerm = SearchTerm, categoryFilter = CategoryFilter, statusFilter = StatusFilter, authorFilter = AuthorFilter, startDate = StartDate?.ToString("yyyy-MM-dd"), endDate = EndDate?.ToString("yyyy-MM-dd"), sortBy = SortBy, sortOrder = SortOrder, currentPage = CurrentPage, pageSize = PageSize });
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string articleId, bool currentStatus)
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
                var articlesResponse = await _apiService.GetByIdAsync<NewsArticleModel>($"/odata/NewsArticles", $"'{articleId}'", "?$expand=Tags");
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

                var result = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{articleId}'", updateData);

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

            return RedirectToPage(new { searchTerm = SearchTerm, categoryFilter = CategoryFilter, statusFilter = StatusFilter, authorFilter = AuthorFilter, startDate = StartDate?.ToString("yyyy-MM-dd"), endDate = EndDate?.ToString("yyyy-MM-dd"), sortBy = SortBy, sortOrder = SortOrder, currentPage = CurrentPage, pageSize = PageSize });
        }

        public async Task<IActionResult> OnPostDuplicateAsync(string articleId)
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
                var duplicateData = new { ArticleId = articleId };
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
                TempData["ErrorMessage"] = $"Error duplicating article: {ex.Message}";
            }

            return RedirectToPage(new { searchTerm = SearchTerm, categoryFilter = CategoryFilter, statusFilter = StatusFilter, authorFilter = AuthorFilter, startDate = StartDate?.ToString("yyyy-MM-dd"), endDate = EndDate?.ToString("yyyy-MM-dd"), sortBy = SortBy, sortOrder = SortOrder, currentPage = CurrentPage, pageSize = PageSize });
        }

        public string GetPageUrl(int pageNumber)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");

            if (CategoryFilter.HasValue)
                queryParams.Add($"categoryFilter={CategoryFilter}");

            if (StatusFilter.HasValue)
                queryParams.Add($"statusFilter={StatusFilter}");

            if (!string.IsNullOrEmpty(AuthorFilter))
                queryParams.Add($"authorFilter={Uri.EscapeDataString(AuthorFilter)}");

            if (StartDate.HasValue)
                queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");

            if (EndDate.HasValue)
                queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");

            if (SortBy != "CreatedDate")
                queryParams.Add($"sortBy={SortBy}");

            if (SortOrder != "desc")
                queryParams.Add($"sortOrder={SortOrder}");

            if (PageSize != 10)
                queryParams.Add($"pageSize={PageSize}");

            queryParams.Add($"currentPage={pageNumber}");

            return $"/Staff/NewsArticles" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }
    }
}