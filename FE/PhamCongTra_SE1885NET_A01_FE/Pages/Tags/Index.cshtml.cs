using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Tags
{
    [Authorize(Roles = "ADMIN,STAFF")]
    public class IndexModel : PageModel
    {
        private readonly ITagService _tagService;

        // 4. Inject Service
        public IndexModel(ITagService tagService)
        {
            _tagService = tagService;
        }

        public IList<Tag> Tag { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                // 5. Gọi Service lấy danh sách thẻ
                // Bạn cần đảm bảo Service đã có hàm GetAllTagsAsync (xem hướng dẫn bên dưới)
                Tag = await _tagService.GetAllTagsAsync();
            }
            catch (Exception ex)
            {
                // 6. Xử lý lỗi
                Tag = new List<Tag>();
                ModelState.AddModelError(string.Empty, "Error retrieving tags: " + ex.Message);
            }
        }
    }
}
