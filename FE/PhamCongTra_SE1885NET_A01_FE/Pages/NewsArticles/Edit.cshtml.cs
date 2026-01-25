using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;

        // 3. Inject Service
        public EditModel(INewsService newsService, ICategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            try
            {
                // 4. Lấy bài viết từ Service
                NewsArticle = await _newsService.GetNewsByIdAsync(id);

                if (NewsArticle == null) return NotFound();

                // 5. CHECK QUYỀN (Authorization)
                // Chỉ Admin hoặc Người tạo ra bài viết mới được sửa
                if (!IsAuthorizedToEdit(NewsArticle))
                {
                    return Forbid();
                }

                // 6. Load Dropdown danh mục
                await LoadCategoryDropdownAsync();

                // Lưu ý: Không load dropdown Account nữa (CreatedBy không được đổi)
            }
            catch
            {
                return NotFound("Error retrieving the news article.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 7. Lấy User ID hiện tại
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            short currentUserId = short.Parse(userIdString);

            // 8. BẢO MẬT: Kiểm tra lại quyền trên Server
            // Phải lấy bài gốc từ DB lên để check người tạo ban đầu (tránh trường hợp user giả mạo form gửi lên)
            var originalArticle = await _newsService.GetNewsByIdAsync(NewsArticle.NewsArticleId);

            if (originalArticle == null) return NotFound();

            if (!IsAuthorizedToEdit(originalArticle))
            {
                return Forbid();
            }

            // 9. Validate
            if (!ModelState.IsValid)
            {
                await LoadCategoryDropdownAsync();
                return Page();
            }

            try
            {
                // 10. Gọi Service update
                // Service sẽ tự set UpdatedBy = currentUserId và ModifiedDate = Now
                await _newsService.UpdateNewsAsync(NewsArticle, currentUserId);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error updating news: " + ex.Message);

                await LoadCategoryDropdownAsync(); // Load lại dropdown để không bị lỗi view
                return Page();
            }
        }

        // --- CÁC HÀM PHỤ TRỢ (HELPER METHODS) ---

        // Hàm kiểm tra quyền: Admin HOẶC Chủ bài viết
        private bool IsAuthorizedToEdit(NewsArticle article)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = short.Parse(userIdString ?? "0");

            var isAdmin = User.IsInRole("ADMIN") || User.FindFirst(ClaimTypes.Role)?.Value == "ADMIN";

            if (isAdmin) return true;
            if (article.CreatedById == userId) return true;

            return false;
        }

        // Hàm load Dropdown Category
        private async Task LoadCategoryDropdownAsync()
        {
            try
            {
                // Load tất cả danh mục (bao gồm cả Inactive nếu đang edit bài cũ thuộc danh mục đó)
                // Hoặc chỉ load Active tùy nghiệp vụ
                var categories = await _categoryService.GetAllCategoriesAsync();

                ViewData["CategoryID"] = new SelectList(categories, "CategoryId", "CategoryName", NewsArticle?.CategoryId);
            }
            catch
            {
                // Xử lý lỗi load dropdown (không làm crash trang)
            }
        }
    }
}
