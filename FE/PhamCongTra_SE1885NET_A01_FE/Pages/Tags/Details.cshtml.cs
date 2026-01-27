using BusinessLogic.Services;
using DataAccess.Models;
using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    public class DetailsModel : PageModel
    {
        private readonly ITagService _tagService;

        public DetailsModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public Tag Tag { get; set; } = new();
        public List<NewsArticle> Articles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Tag = await _tagService.GetByIdAsync(id);
            if (Tag == null)
                return NotFound();

            Articles = await _tagService.GetArticlesByTagAsync(id);
            return Page();
        }
    }
}