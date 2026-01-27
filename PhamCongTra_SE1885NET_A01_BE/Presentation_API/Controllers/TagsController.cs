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

                var createdTag = await _tagService.CreateTagAsync(tag);
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

                var updatedTag = await _tagService.UpdateTagAsync(existingTag);
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

                var success = await _tagService.DeleteTagAsync(key);
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
    }
}