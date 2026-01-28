using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.News
{
    public class SearchModel : PageModel
    {
        private readonly IApiService _apiService;

        public SearchModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PaginatedResult<NewsArticleModel> PaginatedResults { get; set; } = new PaginatedResult<NewsArticleModel>();
        public List<NewsArticleModel> SearchResults { get; set; } = new List<NewsArticleModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? AuthorName { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public short? CategoryId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "date";
        
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 9; // Updated page size

        public int TotalResults { get; set; }
        public bool HasSearched { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Validate pagination parameters
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 9;
            if (PageSize > 24) PageSize = 24;

            // Load categories for filter dropdown
            try
            {
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();
            }
            catch
            {
                Categories = new List<CategoryModel>();
            }

            // Check if search parameters are provided
            HasSearched = !string.IsNullOrEmpty(SearchTerm) || !string.IsNullOrEmpty(AuthorName) ||
                         CategoryId.HasValue || StartDate.HasValue || EndDate.HasValue;

            if (HasSearched)
            {
                await PerformSearchAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return RedirectToPage("/News/Search", new
            {
                SearchTerm,
                AuthorName,
                CategoryId,
                StartDate = StartDate?.ToString("yyyy-MM-dd"),
                EndDate = EndDate?.ToString("yyyy-MM-dd"),
                SortBy,
                SortOrder,
                CurrentPage = 1 // Reset to first page on new search
            });
        }

        private async Task PerformSearchAsync()
        {
            try
            {
                // Use the search endpoint from Functions controller
                var searchUrl = "/odata/NewsArticlesFunctions/Search?" + BuildSearchQuery();
                var searchResponse = await _apiService.GetAsync<NewsArticleModel>(searchUrl);
                var allResults = searchResponse ?? new List<NewsArticleModel>();

                // Filter for active articles only (for public users)
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    allResults = allResults.Where(a => a.NewsStatus == true).ToList();
                }

                // Apply sorting
                var sortedResults = SortResults(allResults);
                TotalResults = sortedResults.Count;

                // Calculate pagination
                var totalPages = (int)Math.Ceiling((double)TotalResults / PageSize);
                
                // Ensure current page doesn't exceed total pages
                if (CurrentPage > totalPages && totalPages > 0)
                {
                    CurrentPage = totalPages;
                }

                // Get items for current page
                var pagedResults = sortedResults
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Set up pagination result
                PaginatedResults = new PaginatedResult<NewsArticleModel>
                {
                    Items = pagedResults,
                    TotalItems = TotalResults,
                    TotalPages = totalPages,
                    CurrentPage = CurrentPage,
                    PageSize = PageSize
                };

                // For backwards compatibility
                SearchResults = pagedResults;

                // Set up pagination info
                Pagination = new PaginationInfo
                {
                    CurrentPage = CurrentPage,
                    TotalPages = totalPages,
                    TotalItems = TotalResults,
                    PageSize = PageSize
                };
            }
            catch (Exception)
            {
                SearchResults = new List<NewsArticleModel>();
                PaginatedResults = new PaginatedResult<NewsArticleModel>();
                Pagination = new PaginationInfo();
                TotalResults = 0;
            }
        }

        private string BuildSearchQuery()
        {
            var queryParts = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParts.Add($"title={Uri.EscapeDataString(SearchTerm)}");

            if (!string.IsNullOrEmpty(AuthorName))
                queryParts.Add($"authorName={Uri.EscapeDataString(AuthorName)}");

            if (CategoryId.HasValue)
            {
                var categoryName = Categories.FirstOrDefault(c => c.CategoryId == CategoryId)?.CategoryName;
                if (!string.IsNullOrEmpty(categoryName))
                    queryParts.Add($"categoryName={Uri.EscapeDataString(categoryName)}");
            }

            if (StartDate.HasValue)
                queryParts.Add($"startDate={StartDate:yyyy-MM-dd}");

            if (EndDate.HasValue)
                queryParts.Add($"endDate={EndDate:yyyy-MM-dd}");

            return string.Join("&", queryParts);
        }

        private List<NewsArticleModel> SortResults(List<NewsArticleModel> results)
        {
            return SortBy.ToLower() switch
            {
                "title" => SortOrder == "desc" 
                    ? results.OrderByDescending(a => a.NewsTitle).ToList()
                    : results.OrderBy(a => a.NewsTitle).ToList(),
                "author" => SortOrder == "desc"
                    ? results.OrderByDescending(a => a.CreatedByName).ToList()
                    : results.OrderBy(a => a.CreatedByName).ToList(),
                "category" => SortOrder == "desc"
                    ? results.OrderByDescending(a => a.CategoryName).ToList()
                    : results.OrderBy(a => a.CategoryName).ToList(),
                _ => SortOrder == "desc" // default: date
                    ? results.OrderByDescending(a => a.CreatedDate).ToList()
                    : results.OrderBy(a => a.CreatedDate).ToList()
            };
        }

        public string GetSearchPageUrl(int pageNum)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
                
            if (!string.IsNullOrEmpty(AuthorName))
                queryParams.Add($"authorName={Uri.EscapeDataString(AuthorName)}");
                
            if (CategoryId.HasValue)
                queryParams.Add($"categoryId={CategoryId}");
                
            if (StartDate.HasValue)
                queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
                
            if (EndDate.HasValue)
                queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
                
            if (SortBy != "date")
                queryParams.Add($"sortBy={SortBy}");
                
            if (SortOrder != "desc")
                queryParams.Add($"sortOrder={SortOrder}");
                
            if (PageSize != 9) // Updated page size check
                queryParams.Add($"pageSize={PageSize}");
                
            queryParams.Add($"currentPage={pageNum}");
            
            return $"/News/Search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }

        public string GetSearchPageSizeUrl(int newPageSize)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
                
            if (!string.IsNullOrEmpty(AuthorName))
                queryParams.Add($"authorName={Uri.EscapeDataString(AuthorName)}");
                
            if (CategoryId.HasValue)
                queryParams.Add($"categoryId={CategoryId}");
                
            if (StartDate.HasValue)
                queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
                
            if (EndDate.HasValue)
                queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
                
            if (SortBy != "date")
                queryParams.Add($"sortBy={SortBy}");
                
            if (SortOrder != "desc")
                queryParams.Add($"sortOrder={SortOrder}");
                
            queryParams.Add($"pageSize={newPageSize}");
            queryParams.Add($"currentPage=1"); // Reset to first page
            
            return $"/News/Search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }
    }
}