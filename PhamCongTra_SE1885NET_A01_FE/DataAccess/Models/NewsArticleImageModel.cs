using System;

namespace DataAccess.Models
{
    public class NewsArticleImageModel
    {
        public int ImageId { get; set; }
        public string NewsArticleId { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string? Caption { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
