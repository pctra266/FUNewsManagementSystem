using DataAccess;
using DataAccess.Models;
using Repositories;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public void AddCategory(Category category)
        {
            _categoryRepository.AddCategory(category);
        }

        public bool CategoryExists(short id)
        {
            return _categoryRepository.CategoryExists(id);
        }

        public void DeleteCategory(short id)
        {
            _categoryRepository.DeleteCategory(id);
        }

        public List<Category> GetActiveCategories()
        {
            return _categoryRepository.GetActiveCategories();
        }

        public List<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        public Category GetCategoryById(short id)
        {
            return _categoryRepository.GetCategoryById(id);
        }

        public void UpdateCategory(Category category)
        {
            _categoryRepository.UpdateCategory(category);
        }
    }
}
