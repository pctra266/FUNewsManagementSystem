using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;

namespace BussinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            SystemAccount? account = null;

            // Check admin account from appsettings.json
            var adminAccount = GetAdminAccount();
            if (adminAccount != null && adminAccount.AccountEmail == email && adminAccount.AccountPassword == password)
            {
                account = adminAccount;
            }
            else
            {
                // Check regular accounts from database
                account = await _unitOfWork.AccountRepository
                    .FindSingleAsync(a => a.AccountEmail == email && a.AccountPassword == password);
            }

            if (account == null)
            {
                return null;
            }

            return GenerateJwtToken(account);
        }

        public async Task<SystemAccount?> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;

                // Check admin account first
                var adminAccount = GetAdminAccount();
                if (adminAccount?.AccountEmail == email)
                {
                    return adminAccount;
                }

                // Check regular accounts
                return await _unitOfWork.AccountRepository
                    .FindSingleAsync(a => a.AccountEmail == email);
            }
            catch
            {
                return null;
            }
        }

        public string GenerateJwtToken(SystemAccount account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var roleValue = account.AccountRole switch
            {
                1 => "1", // Staff
                2 => "2", // Lecturer
                _ => "Admin" // Admin or default
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                    new Claim(ClaimTypes.Name, account.AccountName ?? ""),
                    new Claim(ClaimTypes.Email, account.AccountEmail ?? ""),
                    new Claim(ClaimTypes.Role, roleValue)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public SystemAccount? GetAdminAccount()
        {
            var adminEmail = _configuration["AdminAccount:AccountEmail"];
            var adminPassword = _configuration["AdminAccount:AccountPassword"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                return null;
            }

            return new SystemAccount
            {
                AccountId = 0, // Special ID for admin
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                AccountPassword = adminPassword,
                AccountRole = null // Admin role is handled separately
            };
        }

        public async Task<ClaimsPrincipal?> ValidateJwtTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}