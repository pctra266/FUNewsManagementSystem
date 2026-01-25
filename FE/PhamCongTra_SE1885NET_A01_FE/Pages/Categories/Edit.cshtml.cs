using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    public class EditModel : PageModel
    {
     
            // 3. Khai báo Service
            private readonly ICategoryService _categoryService;

            // 4. Inject Service
            public EditModel(ICategoryService categoryService)
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
                    // 5. Lấy thông tin Category cần sửa
                    Category = await _categoryService.GetCategoryByIdAsync(id.Value);

                    if (Category == null)
                    {
                        return NotFound();
                    }

                    // 6. Lấy danh sách để đổ vào Dropdown (Parent Category)
                    var categories = await _categoryService.GetAllCategoriesAsync();

                    // (Tùy chọn) Logic loại bỏ chính nó khỏi danh sách cha để tránh vòng lặp cha-con
                    // var validParents = categories.Where(c => c.CategoryId != id.Value); 

                    ViewData["ParentCategoryID"] = new SelectList(categories, "CategoryId", "CategoryName", Category.ParentCategoryId);
                }
                catch (Exception)
                {
                    return NotFound("Error retrieving the category. Please try again later.");
                }

                return Page();
            }

            public async Task<IActionResult> OnPostAsync()
            {
                // 7. Validate dữ liệu
                if (!ModelState.IsValid)
                {
                    await LoadDropdownAsync(); // Load lại dropdown nếu form sai
                    return Page();
                }

                try
                {
                    // 8. Gọi Service update
                    await _categoryService.UpdateCategoryAsync(Category);

                    return RedirectToPage("./Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error updating category: " + ex.Message);

                    // 9. QUAN TRỌNG: Khi update lỗi, phải load lại Dropdown
                    // nếu không trang Edit sẽ bị lỗi null view (ViewData rỗng)
                    await LoadDropdownAsync();

                    return Page();
                }
            }

            // Hàm phụ để load Dropdown đỡ phải viết lặp lại code
            private async Task LoadDropdownAsync()
            {
                try
                {
                    var categories = await _categoryService.GetAllCategoriesAsync();
                    ViewData["ParentCategoryID"] = new SelectList(categories, "CategoryId", "CategoryName", Category.ParentCategoryId);
                }
                catch
                {
                    // Nếu load dropdown lỗi thì bỏ qua hoặc log lại
                }
            }
        }
}
