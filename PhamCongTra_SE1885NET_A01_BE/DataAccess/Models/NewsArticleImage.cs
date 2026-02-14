using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class NewsArticleImage
{
    public int ImageId { get; set; }

    public string NewsArticleId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual NewsArticle NewsArticle { get; set; } = null!;
}
