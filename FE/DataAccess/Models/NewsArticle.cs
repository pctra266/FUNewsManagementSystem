using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public partial class NewsArticle
{
    [DisplayName("ID bài viết")]
    [Required]
    [StringLength(20)]
    public string NewsArticleId { get; set; } = null!; // SQL là nvarchar(20)

    [DisplayName("Tiêu đề")]
    [StringLength(400)]
    public string? NewsTitle { get; set; }

    [DisplayName("Dòng tít")]
    [Required]
    [StringLength(150)]
    public string Headline { get; set; } = null!;

    [DisplayName("Ngày tạo")]
    [DataType(DataType.Date)]
    public DateTime? CreatedDate { get; set; }

    [DisplayName("Nội dung")]
    [DataType(DataType.MultilineText)]
    public string? NewsContent { get; set; }

    [DisplayName("Nguồn tin")]
    [StringLength(400)]
    public string? NewsSource { get; set; }

    [DisplayName("Danh mục")]
    public short? CategoryId { get; set; }

    [DisplayName("Trạng thái")]
    public bool? NewsStatus { get; set; } // Active/Inactive

    public short? CreatedById { get; set; }

    public short? UpdatedById { get; set; }

    public DateTime? ModifiedDate { get; set; }

    // --- Navigation Properties (Hứng data từ API) ---

    public Category? Category { get; set; }

    public SystemAccount? CreatedBy { get; set; }

    // List tags để hứng dữ liệu từ bảng NewsTag (Relationship Many-Many)
    public List<Tag> Tags { get; set; } = new List<Tag>();
}
