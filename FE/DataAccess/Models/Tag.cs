using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DataAccess.Models;

public partial class Tag
{
    public int TagId { get; set; }

    [DisplayName("Tên thẻ")]
    public string? TagName { get; set; }

    [DisplayName("Ghi chú")]
    public string? Note { get; set; }
}
