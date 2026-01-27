using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.News
{
    public class ActiveModel : PageModel
    {
        private readonly IApiService _apiService;

        public ActiveModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PaginatedResult<NewsArticleModel> PaginatedArticles { get; set; } = new PaginatedResult<NewsArticleModel>();
        public List<NewsArticleModel> ActiveArticles { get; set; } = new List<NewsArticleModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
        
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;
        
        [BindProperty(SupportsGet = true)]
        public short? SelectedCategoryId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 9; // 3x3 grid layout

        public async Task<IActionResult> OnGetAsync()
        {
            // Validate and set default values
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 9;
            
            // Limit page size to reasonable values
            if (PageSize > 24) PageSize = 24;
            if (PageSize < 3) PageSize = 3;

            try
            {
                // Load categories for filtering using Functions controller
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();

                // Get active articles using Functions controller
                var articlesResponse = await _apiService.GetAsync<NewsArticleModel>("/odata/NewsArticlesFunctions/Active");
                var allArticles = articlesResponse ?? new List<NewsArticleModel>();

                // Apply search and filters
                var filteredArticles = allArticles.AsEnumerable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredArticles = filteredArticles.Where(a => 
                        a.NewsTitle?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        a.NewsContent?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                        a.Headline?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true);
                }

                if (SelectedCategoryId.HasValue)
                {
                    filteredArticles = filteredArticles.Where(a => a.CategoryId == SelectedCategoryId);
                }

                // Order by creation date descending
                filteredArticles = filteredArticles.OrderByDescending(a => a.CreatedDate);

                // Calculate pagination
                var totalItems = filteredArticles.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / PageSize);
                
                // Ensure current page doesn't exceed total pages
                if (CurrentPage > totalPages && totalPages > 0)
                {
                    CurrentPage = totalPages;
                }

                // Get items for current page
                var pagedArticles = filteredArticles
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Set up pagination result
                PaginatedArticles = new PaginatedResult<NewsArticleModel>
                {
                    Items = pagedArticles,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = CurrentPage,
                    PageSize = PageSize
                };

                // For backwards compatibility
                ActiveArticles = pagedArticles;

                // Set up pagination info
                Pagination = new PaginationInfo
                {
                    CurrentPage = CurrentPage,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    PageSize = PageSize
                };
            }
            catch (Exception)
            {
                // Handle error gracefully
                Categories = new List<CategoryModel>();
                ActiveArticles = new List<NewsArticleModel>();
                PaginatedArticles = new PaginatedResult<NewsArticleModel>();
                Pagination = new PaginationInfo();
            }

            return Page();
        }

        public string GetPageUrl(int page)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            
            if (SelectedCategoryId.HasValue)
                queryParams.Add($"selectedCategoryId={SelectedCategoryId}");
            
            if (PageSize != 9) // Only include if different from default
                queryParams.Add($"pageSize={PageSize}");
                
            queryParams.Add($"currentPage={page}");
            
            return $"/News/Active" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }

        public string GetPageSizeUrl(int newPageSize)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            
            if (SelectedCategoryId.HasValue)
                queryParams.Add($"selectedCategoryId={SelectedCategoryId}");
            
            queryParams.Add($"pageSize={newPageSize}");
            queryParams.Add($"currentPage=1"); // Reset to first page when changing page size
            
            return $"/News/Active" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }
    }
}