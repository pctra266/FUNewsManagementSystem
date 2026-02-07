using DataAccess.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BussinessLogic.Services
{
    public interface IAuthService
    {
        Task<(string AccessToken, string RefreshToken)?> AuthenticateAsync(string email, string password);
        Task<SystemAccount?> ValidateTokenAsync(string token);
        string GenerateJwtToken(SystemAccount account);
        string GenerateRefreshToken();
        Task<(string AccessToken, string RefreshToken)?> RefreshTokenAsync(string accessToken, string refreshToken);
        SystemAccount? GetAdminAccount();
        Task<ClaimsPrincipal?> ValidateJwtTokenAsync(string token);
    }
}
