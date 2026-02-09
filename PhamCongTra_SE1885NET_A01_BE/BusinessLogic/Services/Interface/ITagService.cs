using DataAccess.Models;

namespace BussinessLogic.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<IEnumerable<Tag>> SearchTagsAsync(string? tagName = null);
        Task<Tag> CreateTagAsync(Tag tag, short? userId = null);
        Task<Tag> UpdateTagAsync(Tag tag, short? userId = null);
        Task<bool> DeleteTagAsync(int id, short? userId = null);
        Task<bool> CanDeleteTagAsync(int id);
        Task<IEnumerable<NewsArticle>> GetArticlesByTagAsync(int tagId);
        Task<bool> IsTagNameExistAsync(string tagName, int? excludeId = null);
        IQueryable<Tag> GetTagsQueryable();
    }
}