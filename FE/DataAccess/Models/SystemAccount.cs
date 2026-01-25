using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Models;

public partial class SystemAccount
{
    [DisplayName("Account ID")]
    public short AccountId { get; set; }

    [DisplayName("Tên tài khoản")]
    [Required(ErrorMessage = "Tên không được để trống")]
    public string? AccountName { get; set; }

    [DisplayName("Email")]
    [Required]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? AccountEmail { get; set; }

    [DisplayName("Vai trò")]
    public int? AccountRole { get; set; } 

    [DisplayName("Mật khẩu")]
    [Required]
    [DataType(DataType.Password)]
    public string? AccountPassword { get; set; }
}
