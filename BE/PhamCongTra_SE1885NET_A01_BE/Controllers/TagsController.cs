using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;

        public TagsController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            var tags = await Task.Run(() => _tagRepository.GetTags());
            return Ok(tags);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await Task.Run(() => _tagRepository.GetTagById(id));
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTag(int id, Tag tag)
        {
            if (id != tag.TagId)
            {
                return BadRequest();
            }

            try
            {
                await Task.Run(() => _tagRepository.UpdateTag(tag));
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            try
            {
                await Task.Run(() => _tagRepository.AddTag(tag));
            }
            catch (Exception)
            {
                throw;
            }

            return CreatedAtAction("GetTag", new { id = tag.TagId }, tag);
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await Task.Run(() => _tagRepository.GetTagById(id));
            if (tag == null)
            {
                return NotFound();
            }

            await Task.Run(() => _tagRepository.DeleteTag(id));
            return NoContent();
        }
    }
}
