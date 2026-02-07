using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using DataAccess.Models;
using System.ComponentModel.DataAnnotations;

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
        public List<TagModel> Tags { get; set; } = new List<TagModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();

        [BindProperty]
        public NewsArticleCreateInput CreateArticle { get; set; } = new NewsArticleCreateInput();

        [BindProperty]
        public NewsArticleEditInput EditArticle { get; set; } = new NewsArticleEditInput();

        public bool ShowCreateModal { get; set; }
        public bool ShowEditModal { get; set; }

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

            //_apiService.SetAuthToken(token);

            await LoadPageDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(IFormFile? imageFile)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToPage();
            }

            //_apiService.SetAuthToken(token);

            // Manual binding is not needed for file if passed as param, but we need to check ModelState for other fields
            ModelState.Clear();
            if (!TryValidateModel(CreateArticle, nameof(CreateArticle)))
            {
                ShowCreateModal = true;
                await LoadPageDataAsync();
                return Page();
            }

            try
            {
                // Handle Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var stream = imageFile.OpenReadStream())
                    {
                        var imageUrl = await _apiService.UploadImageAsync("/api/NewsArticles/upload-image", stream, imageFile.FileName);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            CreateArticle.NewsSource = imageUrl;
                        }
                    }
                }

                var createData = new
                {
                    NewsTitle = CreateArticle.NewsTitle,
                    Headline = CreateArticle.Headline,
                    NewsContent = CreateArticle.NewsContent,
                    NewsSource = CreateArticle.NewsSource,
                    CategoryId = CreateArticle.CategoryId,
                    NewsStatus = CreateArticle.NewsStatus,
                    TagIds = CreateArticle.SelectedTagIds
                };

                var result = await _apiService.PostAsync<object>("/odata/NewsArticles", createData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article created successfully!";
                    return RedirectToPage(GetRedirectRouteValues());
                }

                ModelState.AddModelError(string.Empty, "Failed to create article. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            ShowCreateModal = true;
            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync(IFormFile? imageFile)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToPage();
            }

            //_apiService.SetAuthToken(token);

            ModelState.Clear();
            if (!TryValidateModel(EditArticle, nameof(EditArticle)))
            {
                ShowEditModal = true;
                await LoadPageDataAsync();
                return Page();
            }

            try
            {
                 // Handle Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var stream = imageFile.OpenReadStream())
                    {
                        var imageUrl = await _apiService.UploadImageAsync("/api/NewsArticles/upload-image", stream, imageFile.FileName);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            EditArticle.NewsSource = imageUrl;
                        }
                    }
                }

                var updateData = new
                {
                    NewsTitle = EditArticle.NewsTitle,
                    Headline = EditArticle.Headline,
                    NewsContent = EditArticle.NewsContent,
                    NewsSource = EditArticle.NewsSource,
                    CategoryId = EditArticle.CategoryId,
                    NewsStatus = EditArticle.NewsStatus,
                    TagIds = EditArticle.SelectedTagIds
                };

                var result = await _apiService.PutAsync<NewsArticleModel>("/odata/NewsArticles", $"'{EditArticle.NewsArticleId}'", updateData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Article updated successfully!";
                    return RedirectToPage(GetRedirectRouteValues());
                }

                ModelState.AddModelError(string.Empty, "Failed to update article. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            ShowEditModal = true;
            await LoadPageDataAsync();
            return Page();
        }

        private async Task LoadPageDataAsync()
        {
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 50) PageSize = 50;

            try
            {
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

                var tagsResponse = await _apiService.GetAsync<TagModel>("/odata/Tags");
                Tags = tagsResponse ?? new List<TagModel>();

                var query = BuildODataQuery();
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles{query}");

                if (articlesResponse != null)
                {
                    var allFilteredArticles = articlesResponse.ToList();
                    HydrateArticles(allFilteredArticles);

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
        }

        private object GetRedirectRouteValues()
        {
            return new
            {
                searchTerm = SearchTerm,
                categoryFilter = CategoryFilter,
                statusFilter = StatusFilter,
                authorFilter = AuthorFilter,
                startDate = StartDate?.ToString("yyyy-MM-dd"),
                endDate = EndDate?.ToString("yyyy-MM-dd"),
                sortBy = SortBy,
                sortOrder = SortOrder,
                currentPage = CurrentPage,
                pageSize = PageSize
            };
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

            //_apiService.SetAuthToken(token);

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

            //_apiService.SetAuthToken(token);

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

            //_apiService.SetAuthToken(token);

            try
            {
                var duplicateData = new { ArticleId = articleId };
                var result = await _apiService.PostAsync<object>("/api/NewsArticlesFunctions/Duplicate", duplicateData);

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

        private static void HydrateArticles(IEnumerable<NewsArticleModel> articles)
        {
            foreach (var article in articles)
            {
                article.HydrateMetadata();
            }
        }
        public async Task<IActionResult> OnPostSuggestTagsAsync([FromBody] TagSuggestionRequest request)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                 //_apiService.SetAuthToken(token);
            }

            try 
            {
                // Call Backend API
                var suggestions = await _apiService.PostAsync<Dictionary<string, double>>("/api/AI/suggest-tags", request);
                return new JsonResult(suggestions ?? new Dictionary<string, double>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class NewsArticleCreateInput
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

    public class NewsArticleEditInput : NewsArticleCreateInput
    {
        [Required(ErrorMessage = "Article ID is required")]
        public string NewsArticleId { get; set; } = string.Empty;
    }

    public class TagSuggestionRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}