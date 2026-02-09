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
    public class TagsController : ODataController
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing tags
        public async Task<IActionResult> Get()
        {
            try
            {
                var tags = _tagService.GetTagsQueryable();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving tags", error = ex.Message });
            }
        }

        [EnableQuery]
        [AllowAnonymous] // Allow public access for viewing specific tag
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(key);
                if (tag == null)
                {
                    return NotFound(new { message = $"Tag with ID {key} not found" });
                }
                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the tag", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TagCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tag = new Tag
                {
                    TagName = createDto.TagName,
                    Note = createDto.Note
                };

                var userId = GetUserId();
                var createdTag = await _tagService.CreateTagAsync(tag, userId);
                return Created($"/odata/Tags({createdTag.TagId})", createdTag);
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
                return StatusCode(500, new { message = "An error occurred while creating the tag", error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromRoute] int key, [FromBody] TagUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingTag = await _tagService.GetTagByIdAsync(key);
                if (existingTag == null)
                {
                    return NotFound(new { message = $"Tag with ID {key} not found" });
                }

                existingTag.TagName = updateDto.TagName;
                existingTag.Note = updateDto.Note;

                var userId = GetUserId();
                var updatedTag = await _tagService.UpdateTagAsync(existingTag, userId);
                return Ok(updatedTag);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the tag", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] int key)
        {
            try
            {
                var canDelete = await _tagService.CanDeleteTagAsync(key);
                if (!canDelete)
                {
                    return Conflict(new { message = "Cannot delete tag because it is used by news articles" });
                }

                var userId = GetUserId();
                var success = await _tagService.DeleteTagAsync(key, userId);
                if (!success)
                {
                    return NotFound(new { message = $"Tag with ID {key} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the tag", error = ex.Message });
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

        [HttpGet("odata/TagsFunctions/Search")]
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

        [HttpGet("odata/TagsFunctions/ArticlesByTag")]
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