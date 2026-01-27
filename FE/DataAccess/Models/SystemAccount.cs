using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class SystemAccount
    {
        [DisplayName("Account ID")]
        public short AccountId { get; set; }

        [DisplayName("Name")]
        [Required(ErrorMessage = "Name is required")]
        public string? AccountName { get; set; }

        [DisplayName("Email")]
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? AccountEmail { get; set; }

        [DisplayName("Role")]
        public short? AccountRole { get; set; }

        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string? AccountPassword { get; set; }

        public string RoleName => AccountRole switch
        {
            1 => "STAFF",
            2 => "LECTURER",
            99 => "ADMIN",
            _ => "UNKNOWN"
        };
    }
}