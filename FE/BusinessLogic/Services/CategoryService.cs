using DataAccess.Models;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            // Có thể thêm logic sort theo tên A-Z tại đây
            var list = await _categoryRepo.GetAllAsync();
            return list.OrderBy(c => c.CategoryName).ToList();
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            var all = await _categoryRepo.GetAllAsync();
            // Logic: Chỉ lấy cái nào IsActive == true
            return all.Where(c => c.IsActive == true).ToList();
        }

        public async Task<Category?> GetCategoryByIdAsync(short id)
        {
            return await _categoryRepo.GetByIdAsync(id);
        }

        public async Task CreateCategoryAsync(Category category)
        {
            // Logic: Tự động set mặc định IsActive nếu chưa có
            if (category.IsActive == null) category.IsActive = true;
            await _categoryRepo.CreateAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await _categoryRepo.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(short id)
        {
            await _categoryRepo.DeleteAsync(id);
        }
    }
}
