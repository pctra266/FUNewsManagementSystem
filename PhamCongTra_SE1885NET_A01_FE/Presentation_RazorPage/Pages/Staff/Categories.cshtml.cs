using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace Presentation_RazorPage.Pages.Staff
{
    public class CategoriesModel : PageModel
    {
        private readonly IApiService _apiService;

        public CategoriesModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public CategoryCreateModel CreateCategory { get; set; } = new CategoryCreateModel();

        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int InactiveCategories { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token) || (userRole != "1" && userRole != "Admin"))
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            await LoadCategoriesAsync();

            return Page();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CategoryModel>("/odata/CategoriesFunctions/Active");
                Categories = response ?? new List<CategoryModel>();
            }
            catch (Exception)
            {
                Categories = new List<CategoryModel>();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload data
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                var result = await _apiService.PostAsync<CategoryModel>("/odata/Categories", new {
                    CategoryName = CreateCategory.CategoryName,
                    CategoryDesciption = CreateCategory.CategoryDesciption,
                    ParentCategoryId = CreateCategory.ParentCategoryId > 0 ? CreateCategory.ParentCategoryId : null,
                    IsActive = CreateCategory.IsActive
                });

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Category created successfully!";
                    CreateCategory = new CategoryCreateModel(); // Reset form
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create category. Name might already exist.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(short id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            _apiService.SetAuthToken(token!);

            try
            {
                var success = await _apiService.DeleteAsync("/odata/Categories", id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete category. It may be used by articles.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(short id, bool currentStatus)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            _apiService.SetAuthToken(token!);

            try
            {
                // Get the category first
                var category = await _apiService.GetByIdAsync<CategoryModel>("/odata/Categories", id);
                if (category != null)
                {
                    var updatedCategory = new {
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName,
                        CategoryDesciption = category.CategoryDesciption,
                        ParentCategoryId = category.ParentCategoryId,
                        IsActive = !currentStatus
                    };

                    var result = await _apiService.PutAsync<CategoryModel>("/odata/Categories", id, updatedCategory);
                    
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = $"Category {(!currentStatus ? "activated" : "deactivated")} successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update category status.";
                    }
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the category.";
            }

            return RedirectToPage();
        }
    }

    public class CategoryCreateModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category description is required")]
        [StringLength(250, ErrorMessage = "Category description cannot exceed 250 characters")]
        public string CategoryDesciption { get; set; } = string.Empty;
        
        public short? ParentCategoryId { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}