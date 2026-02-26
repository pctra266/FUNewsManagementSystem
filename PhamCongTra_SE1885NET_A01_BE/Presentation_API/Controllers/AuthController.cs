using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;

        public AuthController(IAuthService authService, IAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);
                
                if (result == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                SystemAccount? account = null;
                var adminAccount = _authService.GetAdminAccount();
                
                if (adminAccount?.AccountEmail == loginDto.Email)
                {
                    account = adminAccount;
                }
                else
                {
                    account = await _accountService.GetAccountByEmailAsync(loginDto.Email);
                }

                if (account == null)
                {
                    return Unauthorized(new { message = "Account not found" });
                }

                var response = new LoginResponseDto
                {
                    Token = result.Value.AccessToken,
                    RefreshToken = result.Value.RefreshToken,
                    Account = new SystemAccountDto
                    {
                        AccountId = account.AccountId,
                        AccountName = account.AccountName,
                        AccountEmail = account.AccountEmail,
                        AccountRole = account.AccountRole
                    },
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                    //ExpiresAt = DateTime.UtcNow.AddSeconds(30)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during authentication", error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RefreshTokenAsync(tokenRequest.AccessToken, tokenRequest.RefreshToken);

                if (result == null)
                {
                    return BadRequest(new { message = "Invalid client request" });
                }

                return Ok(new
                {
                    Token = result.Value.AccessToken,
                    RefreshToken = result.Value.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                    //ExpiresAt = DateTime.UtcNow.AddSeconds(30),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token refresh", error = ex.Message });
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Token is required" });
                }

                var account = await _authService.ValidateTokenAsync(token);
                if (account == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                return Ok(new
                {
                    valid = true,
                    account = new SystemAccountDto
                    {
                        AccountId = account.AccountId,
                        AccountName = account.AccountName,
                        AccountEmail = account.AccountEmail,
                        AccountRole = account.AccountRole
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token validation", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // For JWT, logout is typically handled client-side by removing the token
            // Server-side logout would require token blacklisting which is not implemented here
            return Ok(new { message = "Logout successful" });
        }
    }
}