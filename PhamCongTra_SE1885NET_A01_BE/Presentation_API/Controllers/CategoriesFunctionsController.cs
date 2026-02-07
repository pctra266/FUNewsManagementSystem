using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesFunctionsController : ODataController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesFunctionsController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("Active")]
        [EnableQuery(PageSize = 50, MaxTop = 200)] // Categories usually have less data
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

        [HttpGet("Search")]
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