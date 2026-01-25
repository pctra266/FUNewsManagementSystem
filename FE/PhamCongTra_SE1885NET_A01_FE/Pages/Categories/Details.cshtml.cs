using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    public class DetailsModel : PageModel
    {
        // 3. Khai báo Service thay vì HttpClient
        private readonly ICategoryService _categoryService;

        // 4. Inject Service qua Constructor
        public DetailsModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(short? id)
        {
            // Kiểm tra ID null
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service để lấy dữ liệu
                // Lưu ý: id.Value vì biến id ở tham số là nullable (short?)
                Category = await _categoryService.GetCategoryByIdAsync(id.Value);

                if (Category == null)
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi (Log lỗi nếu cần thiết)
                return NotFound("Error retrieving the category details. Please try again later.");
            }

            return Page();
        }
    }
}
