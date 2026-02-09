using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Authorize]
    public class SystemAccountsController : ODataController
    {
        private readonly IAccountService _accountService;

        public SystemAccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize(Policy = "AdminOnly")]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            try
            {
                var accounts = _accountService.GetAccountsQueryable()
                    .Select(a => new SystemAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName,
                        AccountEmail = a.AccountEmail,
                        AccountRole = a.AccountRole,
                        ArticleCount = a.NewsArticles.Count
                    });

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving accounts", error = ex.Message });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [EnableQuery]
        public async Task<IActionResult> Get([FromRoute] short key)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Invalid user identification" });
            }

            if (!IsAdmin() && userId != key)
            {
                return Forbid();
            }

            try
            {
                var account = await _accountService.GetAccountsQueryable()
                    .Where(a => a.AccountId == key)
                    .Select(a => new SystemAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName,
                        AccountEmail = a.AccountEmail,
                        AccountRole = a.AccountRole,
                        ArticleCount = a.NewsArticles.Count
                    })
                    .SingleOrDefaultAsync();

                if (account == null)
                {
                    return NotFound(new { message = $"Account with ID {key} not found" });
                }

                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the account", error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SystemAccountCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var account = new SystemAccount
                {
                    AccountName = createDto.AccountName,
                    AccountEmail = createDto.AccountEmail,
                    AccountPassword = createDto.AccountPassword,
                    AccountRole = createDto.AccountRole
                };

                var userId = GetUserId();
                 // If userId is null (shouldn't be since Authorize), we might want to handle it.
                 // But GetUserId() returns null if parsing fails. 
                 // Let's rely on nullable short? which service accepts.
                
                var createdAccount = await _accountService.CreateAccountAsync(account, userId);
                return Created($"/odata/SystemAccounts({createdAccount.AccountId})", createdAccount);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the account", error = ex.Message });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPut]
        public async Task<IActionResult> Put([FromRoute] short key, [FromBody] SystemAccountUpdateDto updateDto)
        {
            Console.WriteLine("============== SYSTEMACCOUNTS PUT REQUEST ==============");
            Console.WriteLine($"Key: {key}");
            Console.WriteLine($"UpdateDto: AccountName={updateDto?.AccountName}, AccountEmail={updateDto?.AccountEmail}, AccountRole={updateDto?.AccountRole}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("? ModelState validation failed:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>())}");
                }
                return BadRequest(ModelState);
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Invalid user identification" });
            }

            var isAdmin = IsAdmin();
            if (!isAdmin && userId != key)
            {
                return Forbid();
            }

            try
            {
                Console.WriteLine($"Looking for account with ID: {key}");
                var existingAccount = await _accountService.GetAccountByIdAsync(key);
                if (existingAccount == null)
                {
                    Console.WriteLine($"? Account with ID {key} not found");
                    return NotFound(new { message = $"Account with ID {key} not found" });
                }

                Console.WriteLine($"? Found existing account: {existingAccount.AccountName} ({existingAccount.AccountEmail})");
                Console.WriteLine($"Updating account fields...");
                
                existingAccount.AccountName = updateDto.AccountName;
                existingAccount.AccountEmail = updateDto.AccountEmail;
                existingAccount.AccountRole = isAdmin ? updateDto.AccountRole : existingAccount.AccountRole;

                Console.WriteLine($"Calling UpdateAccountAsync...");
                var updatedAccount = await _accountService.UpdateAccountAsync(existingAccount, userId);
                Console.WriteLine($"? Account updated successfully: {updatedAccount.AccountName}");
                Console.WriteLine("======================================================");
                
                return Ok(updatedAccount);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"? InvalidOperationException: {ex.Message}");
                Console.WriteLine("======================================================");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? General Exception: {ex.Message}");
                Console.WriteLine($"? Stack Trace: {ex.StackTrace}");
                Console.WriteLine("======================================================");
                return StatusCode(500, new { message = "An error occurred while updating the account", error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromRoute] short key)
        {
            try
            {
                var canDelete = await _accountService.CanDeleteAccountAsync(key);
                if (!canDelete)
                {
                    return Conflict(new { message = "Cannot delete account because it has created news articles" });
                }

                var userId = GetUserId();
                var success = await _accountService.DeleteAccountAsync(key, userId);
                if (!success)
                {
                    return NotFound(new { message = $"Account with ID {key} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the account", error = ex.Message });
            }
        }

        private short? GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet("odata/SystemAccountsFunctions/Search")]
        [Authorize(Policy = "AdminOnly")]
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

        [HttpPost("odata/SystemAccountsFunctions/ChangePassword")]
        [Authorize(Policy = "StaffOnly")]
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

        private bool TryGetUserId(out short userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return short.TryParse(userIdClaim, out userId);
        }

        private bool IsAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}