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

        public PaginatedResult<NewsArticleModel> PaginatedArticles { get; set; } = new PaginatedResult<NewsArticleModel>();
        public List<NewsArticleModel> NewsArticles { get; set; } = new List<NewsArticleModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;
        
        [BindProperty(SupportsGet = true)]
        public short? CategoryFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool? StatusFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string AuthorFilter { get; set; } = string.Empty;
        
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

        // Statistics
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int DraftArticles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in and is staff
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            if (userRole != "1") // Only Staff can manage all news articles
            {
                return RedirectToPage("/Access/Denied");
            }

            // Validate pagination parameters
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 50) PageSize = 50;

            _apiService.SetAuthToken(token);

            try
            {
                // Load categories for filtering
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/Categories");
                Categories = categoriesResponse?.Where(c => c.IsActive == true).ToList() ?? new List<CategoryModel>();

                // Build OData query
                var query = BuildODataQuery();
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles{query}");
                
                if (articlesResponse != null)
                {
                    var allFilteredArticles = articlesResponse.ToList();
                    
                    // Calculate statistics
                    TotalArticles = allFilteredArticles.Count;
                    ActiveArticles = allFilteredArticles.Count(a => a.NewsStatus == true);
                    DraftArticles = allFilteredArticles.Count(a => a.NewsStatus != true);

                    // Apply pagination
                    var totalPages = (int)Math.Ceiling((double)TotalArticles / PageSize);
                    if (CurrentPage > totalPages && totalPages > 0)
                    {
                        CurrentPage = totalPages;
                    }

                    var pagedArticles = allFilteredArticles
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();

                    // Set up pagination result
                    PaginatedArticles = new PaginatedResult<NewsArticleModel>
                    {
                        Items = pagedArticles,
                        TotalItems = TotalArticles,
                        TotalPages = totalPages,
                        CurrentPage = CurrentPage,
                        PageSize = PageSize
                    };

                    NewsArticles = pagedArticles;

                    // Set up pagination info
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
                NewsArticles = new List<NewsArticleModel>();
                PaginatedArticles = new PaginatedResult<NewsArticleModel>();
                Pagination = new PaginationInfo();
            }

            return Page();
        }

        private string BuildODataQuery()
        {
            var filters = new List<string>();
            var queryParts = new List<string>();

            // Apply filters
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

            // Build query
            queryParts.Add("$expand=Category,CreatedBy,UpdatedBy");

            if (filters.Any())
            {
                queryParts.Add($"$filter={string.Join(" and ", filters)}");
            }

            // Add sorting
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
                return RedirectToPage("/Access/Denied");
            }

            _apiService.SetAuthToken(token);

            try
            {
                var success = await _apiService.DeleteAsync("/api/NewsArticles", articleId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Article deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete article. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting article: {ex.Message}";
            }

            return RedirectToPage("./Index", new 
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
            });
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