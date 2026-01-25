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
    public class CreateModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;

        // 3. Inject 2 Service cần thiết: 1 để tạo tin, 1 để lấy danh mục
        public CreateModel(INewsService newsService, ICategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoryDropdownAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 4. Lấy ID người dùng hiện tại từ Cookie (Claims)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu mất session/cookie thì đá về login
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }
            short userId = short.Parse(userIdString);

            // 5. Validate dữ liệu
            // Lưu ý: Các trường CreatedDate, CreatedBy không cần validate vì Service sẽ tự điền
            if (!ModelState.IsValid)
            {
                await LoadCategoryDropdownAsync(); // Load lại dropdown nếu lỗi
                return Page();
            }

            try
            {
                // 6. Gọi Service xử lý
                // Service sẽ tự động: Set CreatedDate = Now, Set CreatedBy = userId
                await _newsService.CreateNewsAsync(NewsArticle, userId);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error creating news: " + ex.Message);

                await LoadCategoryDropdownAsync(); // Load lại dropdown để user thử lại
                return Page();
            }
        }

        // Hàm phụ để load Category Dropdown
        private async Task LoadCategoryDropdownAsync()
        {
            try
            {
                // Chỉ lấy các danh mục đang Active để cho người dùng chọn
                var categories = await _categoryService.GetActiveCategoriesAsync();

                // Hiển thị tên danh mục (CategoryName) thay vì mô tả (CategoryDescription) cho chuẩn
                ViewData["CategoryID"] = new SelectList(categories, "CategoryId", "CategoryName");
            }
            catch
            {
                ModelState.AddModelError("", "Cannot load categories.");
            }
        }
    }
}
