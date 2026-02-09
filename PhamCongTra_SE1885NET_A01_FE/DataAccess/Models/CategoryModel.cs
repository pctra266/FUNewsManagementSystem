using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class CategoryModel
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public short? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ArticleCount { get; set; }
    }
}
