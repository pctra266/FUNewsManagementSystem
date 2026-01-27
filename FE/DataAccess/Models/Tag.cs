using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Tag
    {
        public int TagId { get; set; }

        [DisplayName("Tag Name")]
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50)]
        public string? TagName { get; set; }

        [DisplayName("Note")]
        [StringLength(400)]
        public string? Note { get; set; }

        public int ArticleCount { get; set; }
    }
}