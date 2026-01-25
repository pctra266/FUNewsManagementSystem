using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    public class CreateModel : PageModel
    {
        // 1. Chỉ khai báo Service, không dùng HttpClient trực tiếp
        private readonly ICategoryService _categoryService;

        // 2. Inject Service vào Constructor
        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // 3. Gọi Service để lấy dữ liệu cho Dropdown
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["ParentCategoryID"] = new SelectList(categories, "CategoryId", "CategoryName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 4. Validate dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                // Load lại dropdown nếu validate sai (để tránh lỗi null reference trên view)
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewData["ParentCategoryID"] = new SelectList(categories, "CategoryId", "CategoryName");
                return Page();
            }

            try
            {
                // 5. Gọi Service để xử lý nghiệp vụ tạo mới
                // Service sẽ tự gọi xuống Repository -> Repository gọi API
                await _categoryService.CreateCategoryAsync(Category);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 6. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error creating category: " + ex.Message);

                // Load lại dropdown để user thử lại
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewData["ParentCategoryID"] = new SelectList(categories, "CategoryId", "CategoryName");

                return Page();
            }
        }
    }
}
