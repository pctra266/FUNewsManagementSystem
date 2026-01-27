using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Route("odata/[controller]")]
    public class TagsFunctionsController : ODataController
    {
        private readonly ITagService _tagService;

        public TagsFunctionsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet("Search")]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string? tagName)
        {
            try
            {
                var tags = await _tagService.SearchTagsAsync(tagName);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching tags", error = ex.Message });
            }
        }

        [HttpGet("ArticlesByTag")]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetArticlesByTag([FromQuery] int tagId)
        {
            try
            {
                var articles = await _tagService.GetArticlesByTagAsync(tagId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving articles by tag", error = ex.Message });
            }
        }
    }
}