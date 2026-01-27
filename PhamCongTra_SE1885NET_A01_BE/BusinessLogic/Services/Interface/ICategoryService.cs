using DataAccess.Models;

namespace BussinessLogic.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(short id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> SearchCategoriesAsync(string? name = null, string? description = null);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(short id);
        Task<bool> CanDeleteCategoryAsync(short id);
        Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentId);
        Task<int> GetArticleCountByCategoryAsync(short categoryId);
        Task<bool> IsCategoryNameExistAsync(string name, short? excludeId = null);
        IQueryable<Category> GetCategoriesQueryable();
    }
}