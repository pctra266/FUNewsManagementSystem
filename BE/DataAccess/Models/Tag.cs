using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Models;

public partial class Tag
{
    [Key]
    public int TagId { get; set; }

    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
    public string? TagName { get; set; }

    [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
    public string? Note { get; set; }

    [JsonIgnore]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
