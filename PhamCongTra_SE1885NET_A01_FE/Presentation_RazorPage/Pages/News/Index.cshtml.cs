using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public PaginatedResult<NewsArticleModel> PaginatedArticles { get; set; } = new PaginatedResult<NewsArticleModel>();
        public List<NewsArticleModel> ActiveArticles { get; set; } = new List<NewsArticleModel>();
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
        public int PageSize { get; set; } = 9;

        public bool HasFilters { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = 9;

            if (PageSize > 24) PageSize = 24;
            if (PageSize < 3) PageSize = 3;

            try
            {
                var categoriesResponse = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = categoriesResponse ?? new List<CategoryModel>();

                HasFilters = !string.IsNullOrEmpty(SearchTerm) ||
                             !string.IsNullOrEmpty(AuthorName) ||
                             CategoryId.HasValue ||
                             StartDate.HasValue ||
                             EndDate.HasValue;

                var allArticles = await LoadArticlesAsync();

                var activeArticles = allArticles
                    .Where(a => a.NewsStatus == true)
                    .ToList();

                var sortedArticles = SortResults(activeArticles);
                var totalItems = sortedArticles.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / PageSize);

                if (CurrentPage > totalPages && totalPages > 0)
                {
                    CurrentPage = totalPages;
                }

                var pagedArticles = sortedArticles
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                PaginatedArticles = new PaginatedResult<NewsArticleModel>
                {
                    Items = pagedArticles,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = CurrentPage,
                    PageSize = PageSize
                };

                ActiveArticles = pagedArticles;

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
                Categories = new List<CategoryModel>();
                ActiveArticles = new List<NewsArticleModel>();
                PaginatedArticles = new PaginatedResult<NewsArticleModel>();
                Pagination = new PaginationInfo();
            }

            return Page();
        }

        private async Task<List<NewsArticleModel>> LoadArticlesAsync()
        {
            if (HasFilters)
            {
                var searchUrl = "/odata/NewsArticlesFunctions/Search?" + BuildSearchQuery();
                var searchResponse = await _apiService.GetAsync<NewsArticleModel>(searchUrl);
                return searchResponse ?? new List<NewsArticleModel>();
            }

            var articlesResponse = await _apiService.GetAsync<NewsArticleModel>("/odata/NewsArticlesFunctions/Active");
            return articlesResponse ?? new List<NewsArticleModel>();
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
                _ => SortOrder == "desc"
                    ? results.OrderByDescending(a => a.CreatedDate).ToList()
                    : results.OrderBy(a => a.CreatedDate).ToList()
            };
        }

        public string GetPageUrl(int page)
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

            if (PageSize != 9)
                queryParams.Add($"pageSize={PageSize}");

            queryParams.Add($"currentPage={page}");

            return "/News/Index" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }

        public string GetPageSizeUrl(int newPageSize)
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
            queryParams.Add("currentPage=1");
            return "/News/Index" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        }
    }
}