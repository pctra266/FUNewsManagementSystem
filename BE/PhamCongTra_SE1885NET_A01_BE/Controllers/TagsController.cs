using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using DataAccess.Data;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "StaffAccess")]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;
        private readonly NewsContext _context;

        public TagsController(ITagRepository tagRepository, NewsContext context)
        {
            _tagRepository = tagRepository;
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetTags()
        {
            var tags = await Task.Run(() => _tagRepository.GetTags());
            
            // ✅ Include article count per tag
            var result = tags.Select(t => new
            {
                t.TagId,
                t.TagName,
                t.Note,
                ArticleCount = t.NewsArticles.Count
            });
            
            return Ok(result);
        }

        // GET: api/Tags/Search?keyword=tech
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Tag>>> SearchTags([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await Task.Run(() => Ok(_tagRepository.GetTags()));
            }
            
            var tags = await Task.Run(() => _tagRepository.Search(keyword));
            return Ok(tags);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await Task.Run(() => _tagRepository.GetTagById(id));
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }

        // GET: api/Tags/5/Articles
        [HttpGet("{id}/Articles")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetArticlesByTag(int id)
        {
            var tag = await _context.Tags
                .Include(t => t.NewsArticles)
                    .ThenInclude(a => a.Category)
                .Include(t => t.NewsArticles)
                    .ThenInclude(a => a.CreatedBy)
                .FirstOrDefaultAsync(t => t.TagId == id);
            
            if (tag == null)
            {
                return NotFound(new { message = "Tag not found." });
            }
            
            var articles = tag.NewsArticles.Select(a => new
            {
                a.NewsArticleId,
                a.NewsTitle,
                a.Headline,
                a.CreatedDate,
                a.NewsStatus,
                CategoryName = a.Category?.CategoryName,
                AuthorName = a.CreatedBy?.AccountName
            });
            
            return Ok(articles);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(int id, Tag tag)
        {
            if (id != tag.TagId)
            {
                return BadRequest(new { message = "Tag ID mismatch" });
            }

            try
            {
                await Task.Run(() => _tagRepository.UpdateTag(tag));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating tag: {ex.Message}" });
            }
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            // ✅ Validation
            if (string.IsNullOrWhiteSpace(tag.TagName))
            {
                return BadRequest(new { message = "TagName is required." });
            }
            
            try
            {
                await Task.Run(() => _tagRepository.AddTag(tag));
                return CreatedAtAction("GetTag", new { id = tag.TagId }, tag);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating tag: {ex.Message}" });
            }
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await Task.Run(() => _tagRepository.GetTagById(id));
            if (tag == null)
            {
                return NotFound(new { message = "Tag not found." });
            }

            // ✅ Kiểm tra tag có trong NewsTag không
            if (tag.NewsArticles.Any())
            {
                return BadRequest(new { 
                    message = "Cannot delete tag. It is used by news articles.",
                    articleCount = tag.NewsArticles.Count
                });
            }

            try
            {
                await Task.Run(() => _tagRepository.DeleteTag(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting tag: {ex.Message}" });
            }
        }
    }
}
