using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs
{
    public class NewsArticleDto
    {
        public string NewsArticleId { get; set; } = string.Empty;
        public string? NewsTitle { get; set; }
        public string Headline { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string? NewsContent { get; set; }
        public string? NewsSource { get; set; }
        public short? CategoryId { get; set; }
        public bool? NewsStatus { get; set; }
        public short? CreatedById { get; set; }
        public short? UpdatedById { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int ViewCount { get; set; }
        public string? CategoryName { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
        public List<string> TagNames { get; set; } = new List<string>();
        public List<NewsArticleImageDto> Images { get; set; } = new List<NewsArticleImageDto>();
    }

    public class NewsArticleCreateDto
    {
        [Required(ErrorMessage = "News title is required")]
        [StringLength(400, ErrorMessage = "News title cannot exceed 400 characters")]
        public string NewsTitle { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        public string Headline { get; set; } = string.Empty;
        
        [StringLength(4000, ErrorMessage = "News content cannot exceed 4000 characters")]
        public string? NewsContent { get; set; }
        
        [StringLength(400, ErrorMessage = "News source cannot exceed 400 characters")]
        public string? NewsSource { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public short CategoryId { get; set; }
        
        public bool? NewsStatus { get; set; } = true;
        
        public List<int> TagIds { get; set; } = new List<int>();
    }

    public class NewsArticleUpdateDto
    {
        [Required(ErrorMessage = "News title is required")]
        [StringLength(400, ErrorMessage = "News title cannot exceed 400 characters")]
        public string NewsTitle { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        public string Headline { get; set; } = string.Empty;
        
        [StringLength(4000, ErrorMessage = "News content cannot exceed 4000 characters")]
        public string? NewsContent { get; set; }
        
        [StringLength(400, ErrorMessage = "News source cannot exceed 400 characters")]
        public string? NewsSource { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public short CategoryId { get; set; }
        
        public bool? NewsStatus { get; set; }
        
        public List<int> TagIds { get; set; } = new List<int>();
    }
}