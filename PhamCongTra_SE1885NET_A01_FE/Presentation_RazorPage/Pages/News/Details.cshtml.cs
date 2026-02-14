using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.News
{
    public class DetailsModel : PageModel
    {
        private const string ExpandClause = "$expand=Category($select=CategoryName),CreatedBy($select=AccountName),Tags,NewsArticleImages";
        private readonly IApiService _apiService;

        public DetailsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public NewsArticleModel? Article { get; set; }
        public List<NewsArticleModel> RelatedArticles { get; set; } = new List<NewsArticleModel>();
        public List<NewsArticleModel> SameCategoryArticles { get; set; } = new List<NewsArticleModel>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                Article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'", $"?{ExpandClause}");

                if (Article == null)
                {
                    return NotFound();
                }

                Article.HydrateMetadata();

                if (Article.NewsStatus != true)
                {
                    var token = HttpContext.Session.GetString("AuthToken");
                    var userRole = HttpContext.Session.GetString("UserRole");
                    var userId = HttpContext.Session.GetInt32("UserId");

                    if (string.IsNullOrEmpty(token) ||
                        (userRole != "Admin" && Article.CreatedById != userId))
                    {
                        return NotFound();
                    }
                }

                try
                {
                    // Use new Recommendation OData API
                    var relatedResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticles('{id}')/Default.Recommend()");
                    RelatedArticles = relatedResponse ?? new List<NewsArticleModel>();
                    HydrateArticles(RelatedArticles);
                }
                catch
                {
                     // Fallback to Category if API fails
                     if (Article.CategoryId.HasValue)
                     {
                        var sameCategoryResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticlesFunctions/Default.ByCategory(categoryId={Article.CategoryId})?{ExpandClause}");
                        SameCategoryArticles = sameCategoryResponse?
                            .Where(a => a.NewsArticleId != id && a.NewsStatus == true)
                            .Take(3)
                            .ToList() ?? new List<NewsArticleModel>();

                        HydrateArticles(SameCategoryArticles);
                     }
                }
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Page();
        }

        private static void HydrateArticles(IEnumerable<NewsArticleModel> articles)
        {
            foreach (var article in articles)
            {
                article.HydrateMetadata();
            }
        }
    }
}