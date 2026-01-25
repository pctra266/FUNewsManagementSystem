using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        // 1. Khai báo Service thay vì HttpClient
        private readonly ICategoryService _categoryService;

        // 2. Inject Service qua Constructor
        public DeleteModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 3. Gọi Service để lấy thông tin Category cần xóa
                // Lưu ý: id.Value vì id là nullable (short?)
                Category = await _categoryService.GetCategoryByIdAsync(id.Value);

                if (Category == null)
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi (Có thể log ra file hoặc console)
                return NotFound("Error retrieving the category. Please try again later.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 4. Gọi Service để thực hiện xóa
                await _categoryService.DeleteCategoryAsync(id.Value);
            }
            catch (Exception ex)
            {
                // 5. Nếu xóa lỗi, cần hiển thị lại trang và thông báo lỗi
                ModelState.AddModelError(string.Empty, "Error deleting category: " + ex.Message);

                // Quan trọng: Phải load lại dữ liệu Category để hiển thị lại trên form xác nhận xóa
                Category = await _categoryService.GetCategoryByIdAsync(id.Value);

                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
