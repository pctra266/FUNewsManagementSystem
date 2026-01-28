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
                    string query;
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        // Search by name or description with NewsArticles count
                        query = $"/odata/Categories?$filter=contains(tolower(CategoryName), '{SearchTerm.ToLower()}') or contains(tolower(CategoryDesciption), '{SearchTerm.ToLower()}')&$expand=ParentCategory,NewsArticles($select=NewsArticleId)";
                    }
                    else
                    {
                        // Get all categories with parent info and NewsArticles count
                        query = "/odata/Categories?$expand=ParentCategory,NewsArticles($select=NewsArticleId)";
                    }

                    var response = await _apiService.GetAsync<CategoryWithArticlesModel>(query);
                    Categories = response?.Select(c => new CategoryModel
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        CategoryDesciption = c.CategoryDesciption,
                        ParentCategoryId = c.ParentCategoryId,
                        IsActive = c.IsActive,
                        ParentCategoryName = c.ParentCategory?.CategoryName,
                        ArticleCount = c.NewsArticles?.Count ?? 0
                    }).ToList() ?? new List<CategoryModel>();

                    // Calculate statistics
                    TotalCategories = Categories.Count;
                    ActiveCategories = Categories.Count(c => c.IsActive == true);
                    InactiveCategories = Categories.Count(c => c.IsActive != true);
                }
                catch (Exception)
                {
                    Categories = new List<CategoryModel>();
                    TotalCategories = 0;
                    ActiveCategories = 0;
                    InactiveCategories = 0;
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

                    var result = await _apiService.PostAsync<CategoryModel>("/odata/Categories", new
                    {
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
                    var category = await _apiService.GetByIdAsync<CategoryModel>("/odata/Categories", id);
                    if (category != null)
                    {
                        var updatedCategory = new
                        {
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

            public async Task<IActionResult> OnPostUpdateAsync(short CategoryId, string CategoryName, string CategoryDesciption, short? ParentCategoryId, bool IsActive)
            {
                if (string.IsNullOrEmpty(CategoryName) || string.IsNullOrEmpty(CategoryDesciption))
                {
                    TempData["ErrorMessage"] = "Category name and description are required.";
                    return RedirectToPage();
                }

                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                try
                {
                    // Get current category to check if used by articles
                    var currentCategory = await _apiService.GetByIdAsync<CategoryModel>("/odata/Categories", CategoryId);

                    if (currentCategory == null)
                    {
                        TempData["ErrorMessage"] = "Category not found.";
                        return RedirectToPage();
                    }

                    // Check if ParentCategoryId is being changed and category has articles
                    if (currentCategory.ParentCategoryId != ParentCategoryId && currentCategory.ArticleCount > 0)
                    {
                        TempData["ErrorMessage"] = "Cannot change parent category because this category is used by articles.";
                        return RedirectToPage();
                    }

                    var updateData = new
                    {
                        CategoryName = CategoryName,
                        CategoryDesciption = CategoryDesciption,
                        ParentCategoryId = ParentCategoryId > 0 ? ParentCategoryId : null,
                        IsActive = IsActive
                    };

                    var result = await _apiService.PutAsync<CategoryModel>("/odata/Categories", CategoryId, updateData);

                    if (result != null)
                    {
                        TempData["SuccessMessage"] = "Category updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update category. Name might already exist.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
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

        // Helper class to deserialize category with articles
        public class CategoryWithArticlesModel
        {
            public short CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string CategoryDesciption { get; set; } = string.Empty;
            public short? ParentCategoryId { get; set; }
            public bool? IsActive { get; set; }
            public CategoryParentModel? ParentCategory { get; set; }
            public List<ArticleIdModel>? NewsArticles { get; set; }
        }

        public class CategoryParentModel
        {
            public short CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
        }

        public class ArticleIdModel
        {
            public string NewsArticleId { get; set; } = string.Empty;
        }
    }