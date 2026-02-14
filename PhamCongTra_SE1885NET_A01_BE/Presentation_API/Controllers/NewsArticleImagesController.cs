using BussinessLogic.Services;
using DataAccess.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticleImagesController : ControllerBase
    {
        private readonly INewsArticleImageService _imageService;

        public NewsArticleImagesController(INewsArticleImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet("article/{articleId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByArticleId(string articleId)
        {
            var images = await _imageService.GetImagesByArticleIdAsync(articleId);
            return Ok(images);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var image = await _imageService.GetImageByIdAsync(id);
            if (image == null) return NotFound();
            return Ok(image);
        }

        [HttpPost("article/{articleId}")]
        public async Task<IActionResult> Create(string articleId, [FromBody] NewsArticleImageCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var image = await _imageService.AddImageAsync(articleId, dto);
                return CreatedAtAction(nameof(GetById), new { id = image.ImageId }, image);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the image", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewsArticleImageUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedImage = await _imageService.UpdateImageAsync(id, dto);
                return Ok(updatedImage);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the image", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _imageService.DeleteImageAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
