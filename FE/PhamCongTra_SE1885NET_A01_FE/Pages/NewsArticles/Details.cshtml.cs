using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.NewsArticles
{
    public class DetailsModel : PageModel
    {
        // 3. Khai báo Service thay vì HttpClient
        private readonly INewsService _newsService;

        // 4. Inject Service qua Constructor
        public DetailsModel(INewsService newsService)
        {
            _newsService = newsService;
        }

        public NewsArticle NewsArticle { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Kiểm tra ID (Lưu ý ID của NewsArticle là string, không phải short/int)
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service để lấy chi tiết bài viết
                NewsArticle = await _newsService.GetNewsByIdAsync(id);

                if (NewsArticle == null)
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi (Log lỗi nếu cần)
                return NotFound("Error retrieving the news article. Please try again later.");
            }

            return Page();
        }
    }
}
