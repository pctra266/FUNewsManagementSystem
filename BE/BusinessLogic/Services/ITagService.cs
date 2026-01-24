using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ITagService
    {
        public List<Tag> GetTags();
        public void AddTag(Tag tag);
        public void UpdateTag(Tag tag);
        public void DeleteTag(int id);
        public List<Tag> Search(string keyword);
        public Tag GetTagById(int id);
    }
}
