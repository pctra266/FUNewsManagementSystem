using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs
{
    public class NewsArticleImageUpdateDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(400, ErrorMessage = "Image URL cannot exceed 400 characters")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Caption cannot exceed 250 characters")]
        public string? Caption { get; set; }
    }
}
