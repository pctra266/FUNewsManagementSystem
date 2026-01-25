using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly INewsService _newsService;

        public DeleteModel(INewsService newsService)
        {
            _newsService = newsService;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            try
            {
                // 4. Gọi Service lấy bài viết
                NewsArticle = await _newsService.GetNewsByIdAsync(id);

                if (NewsArticle == null) return NotFound();

                // 5. LOGIC PHÂN QUYỀN (AUTHORIZATION)
                // Kiểm tra xem user hiện tại có quyền xóa bài này không
                if (!IsAuthorizedToDelete(NewsArticle))
                {
                    return Forbid(); // Trả về lỗi 403 Forbidden
                }
            }
            catch (Exception)
            {
                return NotFound("Error retrieving the news article.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null) return NotFound();

            try
            {
                // 6. Check lại bài viết tồn tại không và check quyền lần nữa (Bảo mật)
                var articleToCheck = await _newsService.GetNewsByIdAsync(id);

                if (articleToCheck == null) return NotFound();

                if (!IsAuthorizedToDelete(articleToCheck))
                {
                    return Forbid();
                }

                // 7. Gọi Service xóa
                await _newsService.DeleteNewsAsync(id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error deleting news: " + ex.Message);

                // Load lại dữ liệu để hiển thị trang lỗi mà không bị trắng trang
                NewsArticle = await _newsService.GetNewsByIdAsync(id);
                return Page();
            }

            return RedirectToPage("./Index");
        }

        // Hàm phụ để kiểm tra quyền (Admin hoặc Chính chủ)
        private bool IsAuthorizedToDelete(NewsArticle article)
        {
            // Lấy ID user đang đăng nhập
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = short.Parse(userIdString ?? "0");

            // Check Admin
            var isAdmin = User.IsInRole("ADMIN") || User.FindFirst(ClaimTypes.Role)?.Value == "ADMIN";

            // Logic: Nếu là Admin HOẶC là người tạo bài thì OK
            if (isAdmin) return true;
            if (article.CreatedById == userId) return true;

            return false;
        }
    }
}
