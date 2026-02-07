using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public SystemAccountModel Account { get; set; } = new SystemAccountModel();
        public DateTime ExpiresAt { get; set; }
    }
}
