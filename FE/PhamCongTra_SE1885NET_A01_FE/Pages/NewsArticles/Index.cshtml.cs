using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INewsService _newsService;

        // 4. Inject Service
        public IndexModel(INewsService newsService)
        {
            _newsService = newsService;
        }

        public IList<NewsArticle> NewsArticle { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                // 5. Lấy thông tin User từ Claims (Cookie)
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Kiểm tra Role: Dùng IsInRole hoặc check Claim value đều được
                var isAdmin = User.IsInRole("ADMIN") || User.FindFirst(ClaimTypes.Role)?.Value == "ADMIN";

                // 6. Phân luồng dữ liệu
                if (isAdmin)
                {
                    // ADMIN: Xem tất cả bài viết
                    // Gọi Service: GetAllNewsAsync()
                    NewsArticle = await _newsService.GetAllNewsAsync();
                }
                else
                {
                    // STAFF/USER: Chỉ xem bài viết do mình tạo
                    // Parse UserID (short)
                    if (short.TryParse(userIdString, out short userId) && userId > 0)
                    {
                        // Gọi Service: GetMyNewsAsync(userId)
                        // (Hàm này đã được định nghĩa ở bước BusinessLogic trước đó)
                        NewsArticle = await _newsService.GetMyNewsAsync(userId);
                    }
                    else
                    {
                        // Trường hợp không parse được ID hoặc ID user không hợp lệ
                        NewsArticle = new List<NewsArticle>();
                    }
                }
            }
            catch (Exception ex)
            {
                // 7. Xử lý lỗi
                NewsArticle = new List<NewsArticle>();
                ModelState.AddModelError(string.Empty, "Error retrieving news articles: " + ex.Message);
            }
        }
    }
}
