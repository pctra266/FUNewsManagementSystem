using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterActive { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(SearchKeyword) || FilterActive.HasValue)
            {
                Categories = await _categoryService.SearchAsync(SearchKeyword, FilterActive);
            }
            else
            {
                var categoriesWithCount = await _categoryService.GetAllAsync();
                Categories = categoriesWithCount.Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDesciption = c.CategoryDesciption,
                    ParentCategoryId = c.ParentCategoryId,
                    IsActive = c.IsActive,
                    ParentCategoryName = c.ParentCategoryName,
                    ArticleCount = c.ArticleCount
                }).ToList();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(short id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete category: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(short id)
        {
            try
            {
                await _categoryService.ToggleStatusAsync(id);
                TempData["SuccessMessage"] = "Category status updated!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}