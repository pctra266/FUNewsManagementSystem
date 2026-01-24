using DataAccess.Models;
using Repositories;

namespace Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }
        public void AddTag(Tag tag)
        {
            _tagRepository.AddTag(tag);
        }

        public void DeleteTag(int id)
        {
            _tagRepository.DeleteTag(id);
        }

        public List<Tag> GetTags()
        {
            return _tagRepository.GetTags();
        }

        public List<Tag> Search(string keyword)
        {
            return _tagRepository.Search(keyword);
        }

        public void UpdateTag(Tag tag)
        {
            _tagRepository.UpdateTag(tag);
        }

        public Tag GetTagById(int id)
        {
            return _tagRepository.GetTagById(id);
        }
    }
}
