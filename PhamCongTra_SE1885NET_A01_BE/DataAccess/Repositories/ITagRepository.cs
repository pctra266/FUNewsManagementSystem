using DataAccess.Models;

namespace DataAccess.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetMostPopularTagsAsync(int count);
    }
}
