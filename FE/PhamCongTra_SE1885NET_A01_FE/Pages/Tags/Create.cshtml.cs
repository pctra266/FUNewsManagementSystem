using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    [Authorize(Roles = "ADMIN,STAFF")]
    public class CreateModel : PageModel
    {
        private readonly ITagService _tagService;

        // 4. Inject Service
        public CreateModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 5. Gọi Service để tạo mới
                await _tagService.CreateTagAsync(Tag);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 6. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error creating tag: " + ex.Message);
                return Page();
            }
        }
    }
}
