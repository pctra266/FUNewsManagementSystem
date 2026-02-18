using BussinessLogic.Services;
using DataAccess.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticleImagesController : ControllerBase
    {
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif"
        };
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/jpg"
        };
        private static readonly HttpClient ImageValidationHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

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
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var validationResult = await ValidateImagePayloadAsync(dto.ImageUrl, HttpContext.RequestAborted);
            if (validationResult != null) return validationResult;

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
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var validationResult = await ValidateImagePayloadAsync(dto.ImageUrl, HttpContext.RequestAborted);
            if (validationResult != null) return validationResult;

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

        private async Task<IActionResult?> ValidateImagePayloadAsync(string imageUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(new { message = "Image URL is required." });
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            {
                return BadRequest(new { message = "Image URL must be an absolute HTTP or HTTPS address." });
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Image URL must use HTTP or HTTPS." });
            }

            var extension = Path.GetExtension(uri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only JPG, JPEG, PNG or GIF images are allowed." });
            }

            var localFilePath = ResolveLocalUploadsPath(uri);
            if (!string.IsNullOrEmpty(localFilePath) && System.IO.File.Exists(localFilePath))
            {
                var fileInfo = new FileInfo(localFilePath);
                if (fileInfo.Length > MaxImageSizeBytes)
                {
                    return BadRequest(new { message = "Image size must not exceed 5 MB." });
                }

                return null;
            }

            try
            {
                using var response = await SendValidationRequestAsync(uri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = $"Unable to validate image URL (HTTP {(int)response.StatusCode})." });
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.IsNullOrWhiteSpace(mediaType) && !AllowedImageContentTypes.Contains(mediaType))
                {
                    return BadRequest(new { message = $"Image MIME type '{mediaType}' is not allowed." });
                }

                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value > MaxImageSizeBytes)
                {
                    return BadRequest(new { message = "Image size must not exceed 5 MB." });
                }
            }
            catch (TaskCanceledException)
            {
                return BadRequest(new { message = "Timed out while validating image URL." });
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(new { message = "Unable to validate image URL.", error = ex.Message });
            }

            return null;
        }

        private static string? ResolveLocalUploadsPath(Uri uri)
        {
            if (!uri.AbsolutePath.Contains("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var fileName = Path.GetFileName(uri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
        }

        private static async Task<HttpResponseMessage> SendValidationRequestAsync(Uri uri, CancellationToken cancellationToken)
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = await ImageValidationHttpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            {
                response.Dispose();
                var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                return await ImageValidationHttpClient.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }

            return response;
        }
    }
}