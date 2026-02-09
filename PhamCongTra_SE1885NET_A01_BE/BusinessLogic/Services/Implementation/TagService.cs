using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;
using System.Text.Json;

namespace BussinessLogic.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public TagService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles)
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles)
                .FirstOrDefaultAsync(t => t.TagId == id);
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string? tagName = null)
        {
            IQueryable<Tag> query = _unitOfWork.TagRepository.Query();

            if (!string.IsNullOrEmpty(tagName))
            {
                query = query.Where(t => t.TagName!.Contains(tagName));
            }

            return await query.OrderBy(t => t.TagName).ToListAsync();
        }

        public async Task<Tag> CreateTagAsync(Tag tag, short? userId = null)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(tag.TagName))
            {
                throw new ArgumentException("Tag name is required");
            }

            // Check for duplicate name
            if (await IsTagNameExistAsync(tag.TagName))
            {
                throw new InvalidOperationException("Tag name already exists");
            }

            // Assign default active status
            tag.IsActive = true;
            
            await _unitOfWork.TagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            // Audit Log
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "Create", "Tag", tag.TagId.ToString(), null, tag);
            }

            return tag;
        }

        public async Task<Tag> UpdateTagAsync(Tag tag, short? userId = null)
        {
            var existingTag = await _unitOfWork.TagRepository.GetByIdAsync(tag.TagId);
            if (existingTag == null)
            {
                throw new InvalidOperationException("Tag not found");
            }

            // Keep a copy of old values for logging
            var oldTagState = await _unitOfWork.TagRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TagId == tag.TagId);

            // Check for duplicate name (excluding current tag)
            if (await IsTagNameExistAsync(tag.TagName!, tag.TagId))
            {
                throw new InvalidOperationException("Tag name already exists");
            }

            // Update properties
            existingTag.TagName = tag.TagName;
            existingTag.Note = tag.Note;

            _unitOfWork.TagRepository.Update(existingTag);
            await _unitOfWork.SaveChangesAsync();

            // Audit Log
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "Update", "Tag", tag.TagId.ToString(), oldTagState, existingTag);
            }

            return existingTag;
        }

        public async Task<bool> DeleteTagAsync(int id, short? userId = null)
        {
            if (!await CanDeleteTagAsync(id))
            {
                return false;
            }

            var tag = await _unitOfWork.TagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return false;
            }

            // Keep a copy for log
            var tagToLog = new Tag
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                Note = tag.Note,
                IsActive = tag.IsActive
            };

            _unitOfWork.TagRepository.Delete(tag);
            await _unitOfWork.SaveChangesAsync();

            // Audit Log
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "Delete", "Tag", id.ToString(), tagToLog, null);
            }

            return true;
        }

        public async Task<bool> CanDeleteTagAsync(int id)
        {
            // Check if tag is used by any news articles
            var tag = await _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles)
                .FirstOrDefaultAsync(t => t.TagId == id);

            return tag?.NewsArticles?.Count == 0;
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesByTagAsync(int tagId)
        {
            var tag = await _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles)
                    .ThenInclude(n => n.Category)
                .Include(t => t.NewsArticles)
                    .ThenInclude(n => n.CreatedBy)
                .FirstOrDefaultAsync(t => t.TagId == tagId);

            return tag?.NewsArticles ?? new List<NewsArticle>();
        }

        public async Task<bool> IsTagNameExistAsync(string tagName, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.TagRepository
                    .ExistsAsync(t => t.TagName == tagName && t.TagId != excludeId);
            }

            return await _unitOfWork.TagRepository
                .ExistsAsync(t => t.TagName == tagName);
        }

        public IQueryable<Tag> GetTagsQueryable()
        {
            return _unitOfWork.TagRepository.Query()
                .Include(t => t.NewsArticles);
        }
    }
}