using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Route("odata/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class SystemAccountsFunctionsController : ODataController
    {
        private readonly IAccountService _accountService;

        public SystemAccountsFunctionsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("Search")]
        [EnableQuery]
        public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? email, [FromQuery] int? role)
        {
            try
            {
                var query = _accountService.GetAccountsQueryable();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(a => a.AccountName!.Contains(name));
                }

                if (!string.IsNullOrEmpty(email))
                {
                    query = query.Where(a => a.AccountEmail!.Contains(email));
                }

                if (role.HasValue)
                {
                    query = query.Where(a => a.AccountRole == role);
                }

                var accounts = await query
                    .Select(a => new SystemAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName,
                        AccountEmail = a.AccountEmail,
                        AccountRole = a.AccountRole,
                        ArticleCount = a.NewsArticles.Count
                    })
                    .ToListAsync();

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching accounts", error = ex.Message });
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!short.TryParse(userIdClaim, out short userId))
                {
                    return Unauthorized(new { message = "Invalid user identification" });
                }

                var success = await _accountService.ChangePasswordAsync(userId, 
                    changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                
                if (!success)
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing the password", error = ex.Message });
            }
        }
    }
}