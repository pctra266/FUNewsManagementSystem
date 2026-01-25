using DataAccess.Models;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class TagService: ITagService
    {
        private readonly ITagRepository _tagRepo;

        public TagService(ITagRepository tagRepo)
        {
            _tagRepo = tagRepo;
        }

        public async Task CreateTagAsync(Tag tag)
        {
            // Có thể thêm logic validate: Không cho trùng tên Tag
            // var allTags = await _tagRepo.GetAllTagsAsync();
            // if (allTags.Any(t => t.TagName == tag.TagName)) throw new Exception("Tag name exists!");

            await _tagRepo.CreateTagAsync(tag);
        }
        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _tagRepo.GetTagByIdAsync(id);
        }

        public async Task DeleteTagAsync(int id)
        {
            // Có thể thêm logic kiểm tra: Tag đang được sử dụng bởi bài viết nào không?
            // Nếu có thì chặn xóa (tùy nghiệp vụ)

            await _tagRepo.DeleteTagAsync(id);
        }
        public async Task UpdateTagAsync(Tag tag)
        {
            // Có thể thêm validation nếu cần
            await _tagRepo.UpdateTagAsync(tag);
        }
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _tagRepo.GetAllTagsAsync();
        }
    }
}
