using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class NewsArticleCreateInput
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

        public bool NewsStatus { get; set; } = true;

        public List<int> SelectedTagIds { get; set; } = new();
    }

    public class NewsArticleEditInput : NewsArticleCreateInput
    {
        [Required(ErrorMessage = "Article ID is required")]
        public string NewsArticleId { get; set; } = string.Empty;

        public List<NewsArticleImageModel> NewsArticleImages { get; set; } = new();
    }

    public class TagSuggestionRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
