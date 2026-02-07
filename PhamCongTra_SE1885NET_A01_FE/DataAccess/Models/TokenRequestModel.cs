using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class TokenRequestModel
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;
        
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
