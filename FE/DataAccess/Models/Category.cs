using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Models;

public partial class Category
{
    [DisplayName("ID")]
    public short CategoryId { get; set; }

    [DisplayName("Tên danh mục")]
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [DisplayName("Mô tả")]
    [Required]
    [StringLength(250)]
    public string CategoryDesciption { get; set; } = null!;

    [DisplayName("Danh mục cha")]
    public short? ParentCategoryId { get; set; }

    [DisplayName("Kích hoạt")]
    public bool? IsActive { get; set; }

    // Để hiển thị tên danh mục cha (nếu API trả về)
    public Category? ParentCategory { get; set; }

}
