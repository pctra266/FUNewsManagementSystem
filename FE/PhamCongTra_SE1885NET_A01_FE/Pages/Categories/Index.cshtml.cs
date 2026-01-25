using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    [Authorize(Roles = "ADMIN")]
    public class IndexModel : PageModel
    {
        // 3. Khai báo Service thay vì HttpClient
        private readonly ICategoryService _categoryService;

        // 4. Inject Service qua Constructor
        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IList<Category> Category { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                // 5. Gọi Service để lấy toàn bộ danh sách
                // Hàm này sẽ gọi xuống Repository -> Repository gọi API
                var list = await _categoryService.GetAllCategoriesAsync();

                Category = list;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                Category = new List<Category>();
                ModelState.AddModelError(string.Empty, "Error retrieving categories: " + ex.Message);
            }
        }
    }
}
