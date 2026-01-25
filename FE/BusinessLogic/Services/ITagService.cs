using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface ITagService
    {
        Task CreateTagAsync(Tag tag);
        Task<Tag?> GetTagByIdAsync(int id);
        Task DeleteTagAsync(int id);
        Task UpdateTagAsync(Tag tag);
        Task<List<Tag>> GetAllTagsAsync();
    }
}
