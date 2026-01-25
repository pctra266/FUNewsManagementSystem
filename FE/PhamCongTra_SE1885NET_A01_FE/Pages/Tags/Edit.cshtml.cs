using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    [Authorize(Roles = "ADMIN,STAFF")]
    public class EditModel : PageModel
    {
        private readonly ITagService _tagService;

        // 4. Inject Service
        public EditModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        [BindProperty]
        public Tag Tag { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service lấy dữ liệu (Hàm GetById đã làm ở bước trước)
                Tag = await _tagService.GetTagByIdAsync(id.Value);

                if (Tag == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception)
            {
                return NotFound("Error retrieving the tag. Please try again later.");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 6. Gọi Service để cập nhật
                // Bạn cần bổ sung hàm UpdateTagAsync vào Service (xem bên dưới)
                await _tagService.UpdateTagAsync(Tag);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 7. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error updating tag: " + ex.Message);
                return Page();
            }
        }
    }
}
