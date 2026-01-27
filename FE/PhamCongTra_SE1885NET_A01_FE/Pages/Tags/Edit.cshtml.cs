using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    public class EditModel : PageModel
    {
        private readonly ITagService _tagService;

        public EditModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        [BindProperty]
        public Tag Tag { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _tagService.UpdateAsync(Tag);
                TempData["SuccessMessage"] = "Tag updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            return RedirectToPage("./Index");
        }
    }
}