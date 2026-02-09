using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BussinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IAuditService _auditService;

            public CategoryService(IUnitOfWork unitOfWork, IAuditService auditService)
            {
                _unitOfWork = unitOfWork;
                _auditService = auditService;
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
                    query = query.Where(c => c.CategoryDescription!.Contains(description));
                }

                return await query.OrderBy(c => c.CategoryName).ToListAsync();
            }

            public async Task<Category> CreateCategoryAsync(Category category, short? userId = null)
            {
                if (string.IsNullOrEmpty(category.CategoryName))
                    throw new ArgumentException("Category name is required");

                if (string.IsNullOrEmpty(category.CategoryDescription))
                    throw new ArgumentException("Category description is required");

                if (await IsCategoryNameExistAsync(category.CategoryName))
                    throw new InvalidOperationException("Category name already exists");

                // Remove the manual ID generation - let the database handle it
                category.CategoryId = 0; // Reset to default

                await _unitOfWork.CategoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                // Audit Log
                if (userId.HasValue)
                {
                    await _auditService.LogAsync(userId.Value, "Create", "Category", category.CategoryId.ToString(), null, category);
                }

                return category;
            }

            public async Task<Category> UpdateCategoryAsync(Category category, short? userId = null)
            {
                var existingCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(category.CategoryId);
                if (existingCategory == null)
                {
                    throw new InvalidOperationException("Category not found");
                }

                // Keep a copy of old values for logging
                var oldCategoryState = await _unitOfWork.CategoryRepository.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

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
                existingCategory.CategoryDescription = category.CategoryDescription;
                existingCategory.ParentCategoryId = category.ParentCategoryId;
                existingCategory.IsActive = category.IsActive;

                _unitOfWork.CategoryRepository.Update(existingCategory);
                await _unitOfWork.SaveChangesAsync();

                // Audit Log
                if (userId.HasValue)
                {
                    await _auditService.LogAsync(userId.Value, "Update", "Category", category.CategoryId.ToString(), oldCategoryState, existingCategory);
                }

                return existingCategory;
            }

            public async Task<bool> DeleteCategoryAsync(short id, short? userId = null)
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

                // Keep a copy for log
                var categoryToLog = new Category 
                { 
                    CategoryId = category.CategoryId, 
                    CategoryName = category.CategoryName,
                    CategoryDescription = category.CategoryDescription,
                    IsActive = category.IsActive
                };

                _unitOfWork.CategoryRepository.Delete(category);
                await _unitOfWork.SaveChangesAsync();

                // Audit Log
                if (userId.HasValue)
                {
                    await _auditService.LogAsync(userId.Value, "Delete", "Category", id.ToString(), categoryToLog, null);
                }

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
                return _unitOfWork.CategoryRepository.Query();
            }
    }
}
