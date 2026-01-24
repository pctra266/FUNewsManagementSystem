using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Repositories;
using DataAccess.Models;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ISystemAccountRepository _accountRepo;

        public AuthController(IConfiguration config, ISystemAccountRepository accountRepo)
        {
            _config = config;
            _accountRepo = accountRepo;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            
            var adminSection = _config.GetSection("AdminAccount");
            var adminEmail = adminSection["Email"];
            var adminPassword = adminSection["Password"];

            if (request.Email == adminEmail && request.Password == adminPassword)
            {
                var adminUser = new SystemAccount
                {
                    AccountId = 0,
                    AccountName = "System Administrator",
                    AccountEmail = adminEmail,
                    AccountRole = 99, 
                    AccountPassword = adminPassword
                };
                var token = GenerateJwtToken(adminUser, "ADMIN");
                return Ok(new { token });
            }

         
            var user = _accountRepo
                .GetSystemAccounts()
                .FirstOrDefault(u => u.AccountEmail == request.Email && u.AccountPassword == request.Password);

            if (user == null)
                return Unauthorized("Invalid email or password");

            string role = user.AccountRole == 1 ? "STAFF" : "EDITOR";

            var jwt = GenerateJwtToken(user, role);
            return Ok(new { token = jwt });
        }


        private string GenerateJwtToken(SystemAccount user, string role)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.AccountEmail ?? ""),
        new Claim("UserId", user.AccountId.ToString()),
        new Claim(ClaimTypes.Role, role), // 👈 Role claim chuẩn
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
