using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BussinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.CategoryRepository.Query()
                .Include(c => c.ParentCategory)
                .Include(c => c.InverseParentCategory)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(short id)
        {
            return await _unitOfWork.CategoryRepository.Query()
                .Include(c => c.ParentCategory)
                .Include(c => c.InverseParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _unitOfWork.CategoryRepository.Query()
                .Where(c => c.IsActive == true)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string? name = null, string? description = null)
        {
            IQueryable<Category> query = _unitOfWork.CategoryRepository.Query()
                .Include(c => c.ParentCategory);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.CategoryName!.Contains(name));
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(c => c.CategoryDesciption!.Contains(description));
            }

            return await query.OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(category.CategoryName))
            {
                throw new ArgumentException("Category name is required");
            }

            if (string.IsNullOrEmpty(category.CategoryDesciption))
            {
                throw new ArgumentException("Category description is required");
            }

            // Check for duplicate name
            if (await IsCategoryNameExistAsync(category.CategoryName))
            {
                throw new InvalidOperationException("Category name already exists");
            }

            // Generate new ID
            var allCategories = await _unitOfWork.CategoryRepository.GetAllAsync();
            category.CategoryId = (short)(allCategories.Any() ? allCategories.Max(c => c.CategoryId) + 1 : 1);

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(category.CategoryId);
            if (existingCategory == null)
            {
                throw new InvalidOperationException("Category not found");
            }

            // Check if ParentCategoryID can be changed
            if (existingCategory.ParentCategoryId != category.ParentCategoryId)
            {
                var hasArticles = await _unitOfWork.NewsArticleRepository
                    .ExistsAsync(n => n.CategoryId == category.CategoryId);
                
                if (hasArticles)
                {
                    throw new InvalidOperationException("Cannot change ParentCategoryID because this category is used by articles");
                }
            }

            // Check for duplicate name (excluding current category)
            if (await IsCategoryNameExistAsync(category.CategoryName!, category.CategoryId))
            {
                throw new InvalidOperationException("Category name already exists");
            }

            // Update properties
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDesciption = category.CategoryDesciption;
            existingCategory.ParentCategoryId = category.ParentCategoryId;
            existingCategory.IsActive = category.IsActive;

            _unitOfWork.CategoryRepository.Update(existingCategory);
            await _unitOfWork.SaveChangesAsync();

            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(short id)
        {
            if (!await CanDeleteCategoryAsync(id))
            {
                return false;
            }

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            _unitOfWork.CategoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CanDeleteCategoryAsync(short id)
        {
            // Check if category is used by any news articles
            return !await _unitOfWork.NewsArticleRepository.ExistsAsync(n => n.CategoryId == id);
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentId)
        {
            return await _unitOfWork.CategoryRepository.Query()
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<int> GetArticleCountByCategoryAsync(short categoryId)
        {
            return await _unitOfWork.NewsArticleRepository
                .CountAsync(n => n.CategoryId == categoryId);
        }

        public async Task<bool> IsCategoryNameExistAsync(string name, short? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.CategoryRepository
                    .ExistsAsync(c => c.CategoryName == name && c.CategoryId != excludeId);
            }

            return await _unitOfWork.CategoryRepository
                .ExistsAsync(c => c.CategoryName == name);
        }

        public IQueryable<Category> GetCategoriesQueryable()
        {
            return _unitOfWork.CategoryRepository.Query()
                .Include(c => c.ParentCategory)
                .Include(c => c.InverseParentCategory);
        }
    }
}