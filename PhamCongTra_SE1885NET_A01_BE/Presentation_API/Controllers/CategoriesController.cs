using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class CategoriesController : ODataController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing categories
        public async Task<IActionResult> Get()
        {
            try
            {
                var categories = _categoryService.GetCategoriesQueryable();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving categories", error = ex.Message });
            }
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing categories
        public async Task<IActionResult> Get([FromRoute] short key)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(key);
                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {key} not found" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the category", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var createdCategory = await _categoryService.CreateCategoryAsync(category, userId);
                return Created($"/odata/Categories({createdCategory.CategoryId})", createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the category", error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromRoute] short key, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                category.CategoryId = key;
                var updatedCategory = await _categoryService.UpdateCategoryAsync(category, userId);
                return Ok(updatedCategory);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the category", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] short key)
        {
            try
            {
                var canDelete = await _categoryService.CanDeleteCategoryAsync(key);
                if (!canDelete)
                {
                    return Conflict(new { message = "Cannot delete category because it is used by news articles" });
                }

                var userId = GetUserId();
                var success = await _categoryService.DeleteCategoryAsync(key, userId);
                if (!success)
                {
                    return NotFound(new { message = $"Category with ID {key} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the category", error = ex.Message });
            }
        }

        private short? GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet("odata/CategoriesFunctions/Active")]
        [EnableQuery(PageSize = 50, MaxTop = 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var activeCategories = await _categoryService.GetActiveCategoriesAsync();
                return Ok(activeCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active categories", error = ex.Message });
            }
        }

        [HttpGet("odata/CategoriesFunctions/Search")]
        [EnableQuery(PageSize = 50, MaxTop = 200)]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? description)
        {
            try
            {
                var categories = await _categoryService.SearchCategoriesAsync(name, description);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching categories", error = ex.Message });
            }
        }
    }
}