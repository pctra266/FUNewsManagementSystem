using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class NewsArticle
    {
        [DisplayName("Article ID")]
        [Required]
        [StringLength(20)]
        public string NewsArticleId { get; set; } = null!;

        [DisplayName("Title")]
        [StringLength(400)]
        public string? NewsTitle { get; set; }

        [DisplayName("Headline")]
        [Required]
        [StringLength(150)]
        public string Headline { get; set; } = null!;

        [DisplayName("Created Date")]
        [DataType(DataType.Date)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Content")]
        [DataType(DataType.MultilineText)]
        public string? NewsContent { get; set; }

        [DisplayName("Source")]
        [StringLength(400)]
        public string? NewsSource { get; set; }

        [DisplayName("Category")]
        public short? CategoryId { get; set; }

        [DisplayName("Status")]
        public bool? NewsStatus { get; set; }

        public short? CreatedById { get; set; }
        public short? UpdatedById { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public Category? Category { get; set; }
        public SystemAccount? CreatedBy { get; set; }
        public List<Tag> Tags { get; set; } = new();
    }
}