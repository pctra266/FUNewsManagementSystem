using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    [Authorize(Roles = "ADMIN,STAFF")]
    public class DetailsModel : PageModel
    {
        private readonly ITagService _tagService;

        // 4. Inject Service
        public DetailsModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public Tag Tag { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service để lấy thông tin Tag theo ID
                // Hàm GetTagByIdAsync này đã được thêm vào Service ở bước trước (khi làm trang Delete)
                Tag = await _tagService.GetTagByIdAsync(id.Value);

                if (Tag == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception)
            {
                // Xử lý lỗi (Log nếu cần)
                return NotFound("Error retrieving the tag. Please try again later.");
            }
        }
    }
}
