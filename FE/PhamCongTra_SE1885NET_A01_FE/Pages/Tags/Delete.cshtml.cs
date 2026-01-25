using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    [Authorize(Roles = "ADMIN,STAFF")]
    public class DeleteModel : PageModel
    {
        private readonly ITagService _tagService;

        // 4. Inject Service
        public DeleteModel(ITagService tagService)
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
                // 5. Gọi Service lấy thông tin Tag
                // Lưu ý: id.Value vì biến id là nullable (int?)
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 6. Gọi Service thực hiện xóa
                await _tagService.DeleteTagAsync(id.Value);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 7. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error deleting the tag: " + ex.Message);

                // QUAN TRỌNG: Phải load lại dữ liệu Tag để hiển thị lại trang xác nhận (tránh lỗi null view)
                try
                {
                    Tag = await _tagService.GetTagByIdAsync(id.Value);
                }
                catch
                {
                    return NotFound();
                }

                return Page();
            }
        }
    }
}
