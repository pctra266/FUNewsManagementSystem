using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Category
    {
        [DisplayName("ID")]
        public short CategoryId { get; set; }

        [DisplayName("Category Name")]
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string CategoryName { get; set; } = null!;

        [DisplayName("Description")]
        [Required]
        [StringLength(250)]
        public string CategoryDesciption { get; set; } = null!;

        [DisplayName("Parent Category")]
        public short? ParentCategoryId { get; set; }

        [DisplayName("Active")]
        public bool? IsActive { get; set; }

        public Category? ParentCategory { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ArticleCount { get; set; }
    }
}