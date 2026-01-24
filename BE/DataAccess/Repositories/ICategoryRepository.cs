using DataAccess.Models;

namespace Repositories
{
    public interface ICategoryRepository
    {
        List<Category> GetCategories();
        Category GetCategoryById(short id);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(short id);
        public bool CategoryExists(short id);
        List<Category> GetActiveCategories();
    }
}
