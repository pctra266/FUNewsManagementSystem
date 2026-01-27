using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public partial class NewsArticle
{
    [Key]
    [Required]
    [StringLength(20)]
    public string NewsArticleId { get; set; } = null!;

    [StringLength(400)]
    public string? NewsTitle { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 10)]
    public string Headline { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    [StringLength(4000)]
    public string? NewsContent { get; set; }

    [StringLength(400)]
    [Url]
    public string? NewsSource { get; set; }

    [Range(1, short.MaxValue)]
    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; }

    [Range(1, short.MaxValue)]
    public short? CreatedById { get; set; }

    [Range(1, short.MaxValue)]
    public short? UpdatedById { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Category? Category { get; set; }

    public virtual SystemAccount? CreatedBy { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
