using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs
{
    public class NewsArticleImageDto
    {
        public int ImageId { get; set; }
        public string NewsArticleId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class NewsArticleImageCreateDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(400, ErrorMessage = "Image URL cannot exceed 400 characters")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Caption cannot exceed 250 characters")]
        public string? Caption { get; set; }
    }
}
