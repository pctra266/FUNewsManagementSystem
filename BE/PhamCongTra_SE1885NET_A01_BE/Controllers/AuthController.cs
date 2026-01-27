using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Repositories;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

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
            // Check admin account
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
                    AccountRole = 99
                };
                var token = GenerateJwtToken(adminUser);
                return Ok(new { token, role = "ADMIN", userName = adminUser.AccountName });
            }

            // Check database accounts
            var user = _accountRepo
                .GetSystemAccounts()
                .FirstOrDefault(u => u.AccountEmail == request.Email && u.AccountPassword == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var jwt = GenerateJwtToken(user);
            var roleName = GetRoleName((short?)user.AccountRole);
            
            return Ok(new { token = jwt, role = roleName, userName = user.AccountName });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                userId,
                email,
                role,
                isAdmin = User.IsInRole("ADMIN"),
                isStaff = User.IsInRole("STAFF"),
                isLecturer = User.IsInRole("LECTURER")
            });
        }

        private string GenerateJwtToken(SystemAccount user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            // Map AccountRole to Role Name
            var roleName = GetRoleName((short?)user.AccountRole);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.AccountEmail ?? ""),
                new Claim("UserId", user.AccountId.ToString()),
                new Claim("UserName", user.AccountName ?? ""),
                new Claim(ClaimTypes.Role, roleName),
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

        // ✅ Helper method: Map AccountRole (number) -> Role Name (string)
        private string GetRoleName(short? accountRole)
        {
            return accountRole switch
            {
                1 => "STAFF",      // Staff - Manage categories and articles
                2 => "LECTURER",   // Lecturer - Read and search only
                99 => "ADMIN",     // Admin - Full control
                _ => "LECTURER"    // Default to most restrictive
            };
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
