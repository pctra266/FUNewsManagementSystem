using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages.News
{
    public class DetailsModel : PageModel
    {
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
                // Get the article (no authentication needed for active articles)
                Article = await _apiService.GetByIdAsync<NewsArticleModel>("/odata/NewsArticles", $"'{id}'");
                
                if (Article == null)
                {
                    return NotFound();
                }

                // Only show active articles to public
                if (Article.NewsStatus != true)
                {
                    // Check if user is logged in and has permission to view drafts
                    var token = HttpContext.Session.GetString("AuthToken");
                    var userRole = HttpContext.Session.GetString("UserRole");
                    var userId = HttpContext.Session.GetInt32("UserId");
                    
                    if (string.IsNullOrEmpty(token) || 
                        (userRole != "Admin" && Article.CreatedById != userId))
                    {
                        return NotFound();
                    }
                }

                // Get related articles
                try
                {
                    var relatedResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticlesFunctions/Related?articleId={id}&limit=3");
                    RelatedArticles = relatedResponse ?? new List<NewsArticleModel>();
                }
                catch
                {
                    // Fallback to same category articles
                    if (Article.CategoryId.HasValue)
                    {
                        var sameCategoryResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticlesFunctions/ByCategory?categoryId={Article.CategoryId}");
                        SameCategoryArticles = sameCategoryResponse?
                            .Where(a => a.NewsArticleId != id && a.NewsStatus == true)
                            .Take(3)
                            .ToList() ?? new List<NewsArticleModel>();
                    }
                }
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Page();
        }
    }
}