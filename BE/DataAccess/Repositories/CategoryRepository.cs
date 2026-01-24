using DataAccess.Data; // Namespace chứa NewsContext
using DataAccess.Models;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC: Để dùng .Include()

namespace Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly NewsContext _context;

        public CategoryRepository(NewsContext context)
        {
            _context = context;
        }

        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public bool CategoryExists(short id)
        {
            return _context.Categories.Any(c => c.CategoryId == id);
        }

        public void DeleteCategory(short id)
        {
            // Tìm category trước khi xóa
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }

        public List<Category> GetActiveCategories()
        {
            // Lấy danh sách category đang hoạt động
            return _context.Categories
                           .Where(c => c.IsActive == true)
                           .ToList();
        }

        public List<Category> GetCategories()
        {
            // Lấy toàn bộ và Include thông tin Category cha (nếu có)
            return _context.Categories
                           .Include(c => c.ParentCategory)
                           .ToList();
        }

        public Category GetCategoryById(short id)
        {
            // Lấy 1 category theo ID và Include cha
            return _context.Categories
                           .Include(c => c.ParentCategory)
                           .FirstOrDefault(c => c.CategoryId == id);
        }

        public void UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }
    }
}