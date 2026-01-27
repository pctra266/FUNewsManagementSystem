using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    public class IndexModel : PageModel
    {
        private readonly ITagService _tagService;

        public IndexModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public List<Tag> Tags { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        public async Task OnGetAsync()
        {
            Tags = await _tagService.SearchAsync(SearchKeyword);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _tagService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Tag deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete tag: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}