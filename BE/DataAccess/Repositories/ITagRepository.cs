using DataAccess.Models;
using DataAccess.Data;

namespace Repositories
{
    public interface ITagRepository
    {
        public List<Tag> GetTags();
        public void AddTag(Tag tag);
        public void UpdateTag(Tag tag);
        public void DeleteTag(int id);
        public List<Tag> Search(string keyword);
        public Tag GetTagById(int id);
    }
}
