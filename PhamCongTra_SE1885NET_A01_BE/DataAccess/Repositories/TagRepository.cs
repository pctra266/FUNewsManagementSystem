using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(NewsContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetMostPopularTagsAsync(int count)
        {
            // Logic: Order by usage count in NewsArticles
            // Note: Since we have a many-to-many relationship, we can count the NewsArticles collection
            return await _dbSet
                .Include(t => t.NewsArticles)
                .OrderByDescending(t => t.NewsArticles.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}
