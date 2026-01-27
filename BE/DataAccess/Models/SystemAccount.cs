using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Models;

public partial class SystemAccount
{
    [Key]
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Account name must be between 2 and 100 characters")]
    public string? AccountName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
    public string? AccountEmail { get; set; }

    [Required(ErrorMessage = "Account role is required")]
    [Range(1, 99, ErrorMessage = "Account role must be between 1 and 99")]
    public int? AccountRole { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(70, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 70 characters")]
    [DataType(DataType.Password)]
    public string? AccountPassword { get; set; }

    [JsonIgnore]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
