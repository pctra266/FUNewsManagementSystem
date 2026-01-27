using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "StaffAccess")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly INewsArticleRepository _newsArticleRepository;

        public CategoriesController(
            ICategoryRepository categoryRepository,
            INewsArticleRepository newsArticleRepository)
        {
            _categoryRepository = categoryRepository;
            _newsArticleRepository = newsArticleRepository;
        }

        // GET: api/Categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var categories = await Task.Run(() => _categoryRepository.GetCategories());
            
            // ✅ Include article count per category
            var result = categories.Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.CategoryDesciption,
                c.ParentCategoryId,
                c.IsActive,
                ParentCategoryName = c.ParentCategory?.CategoryName,
                ArticleCount = _newsArticleRepository.GetNewsArticlesByCategory(c.CategoryId).Count
            });
            
            return Ok(result);
        }

        // GET: api/Categories/Search?keyword=tech&isActive=true
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Category>>> SearchCategories(
            [FromQuery] string? keyword,
            [FromQuery] bool? isActive)
        {
            var categories = await Task.Run(() => _categoryRepository.GetCategories());
            
            if (!string.IsNullOrEmpty(keyword))
            {
                categories = categories.Where(c => 
                    c.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (c.CategoryDesciption != null && c.CategoryDesciption.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (isActive.HasValue)
            {
                categories = categories.Where(c => c.IsActive == isActive.Value).ToList();
            }
            
            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetCategory(short id)
        {
            var category = await Task.Run(() => _categoryRepository.GetCategoryById(id));
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(short id, Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new { message = "Category ID mismatch" });
            }

            // ✅ Kiểm tra ParentCategoryID không được đổi nếu đã có articles
            var existingCategory = await Task.Run(() => _categoryRepository.GetCategoryById(id));
            if (existingCategory == null)
            {
                return NotFound();
            }
            
            var articles = await Task.Run(() => _newsArticleRepository.GetNewsArticlesByCategory(id));
            
            if (articles.Any() && existingCategory.ParentCategoryId != category.ParentCategoryId)
            {
                return BadRequest(new { 
                    message = "Cannot change ParentCategoryID. This category contains news articles.",
                    articleCount = articles.Count
                });
            }

            try
            {
                await Task.Run(() => _categoryRepository.UpdateCategory(category));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating category: {ex.Message}" });
            }

            return NoContent();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            // ✅ Validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                return BadRequest(new { message = "CategoryName is required." });
            }
            
            if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
            {
                return BadRequest(new { message = "CategoryDescription is required." });
            }
            
            try
            {
                await Task.Run(() => _categoryRepository.AddCategory(category));
                return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating category: {ex.Message}" });
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(short id)
        {
            var category = await Task.Run(() => _categoryRepository.GetCategoryById(id));
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            // ✅ Kiểm tra category có articles không
            var articles = await Task.Run(() => 
                _newsArticleRepository.GetNewsArticlesByCategory(id));
            
            if (articles.Any())
            {
                return BadRequest(new { 
                    message = "Cannot delete category. It contains news articles.",
                    articleCount = articles.Count
                });
            }

            try
            {
                await Task.Run(() => _categoryRepository.DeleteCategory(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting category: {ex.Message}" });
            }
        }

        // PUT: api/Categories/5/ToggleStatus
        [HttpPut("{id}/ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(short id)
        {
            var category = await Task.Run(() => _categoryRepository.GetCategoryById(id));
            if (category == null)
            {
                return NotFound();
            }
            
            category.IsActive = !category.IsActive;
            
            try
            {
                await Task.Run(() => _categoryRepository.UpdateCategory(category));
                return Ok(new { 
                    message = "Category status updated successfully.",
                    isActive = category.IsActive
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error toggling status: {ex.Message}" });
            }
        }
    }
}
