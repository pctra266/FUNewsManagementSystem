using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class SystemAccountModel
    {
        public short AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? AccountEmail { get; set; }
        public int? AccountRole { get; set; }
        public string RoleName => AccountRole switch
        {
            1 => "Staff",
            2 => "Lecturer",
            _ => "Admin"
        };
        public int ArticleCount { get; set; }
    }
}
