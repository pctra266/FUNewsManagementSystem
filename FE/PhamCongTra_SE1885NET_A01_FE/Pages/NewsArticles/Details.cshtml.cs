using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    public class DetailsModel : PageModel
    {
        private readonly INewsArticleService _newsService;
        private readonly INewsTagService _newsTagService;

        public DetailsModel(
            INewsArticleService newsService,
            INewsTagService newsTagService)
        {
            _newsService = newsService;
            _newsTagService = newsTagService;
        }

        public NewsArticle NewsArticle { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
        public List<NewsArticle> RelatedArticles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            NewsArticle = await _newsService.GetByIdAsync(id);
            if (NewsArticle == null)
                return NotFound();

            Tags = await _newsTagService.GetTagsByArticleAsync(id);
            RelatedArticles = await _newsService.GetRelatedAsync(id);

            return Page();
        }
    }
}