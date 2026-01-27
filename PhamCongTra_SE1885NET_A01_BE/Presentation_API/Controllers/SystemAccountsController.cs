using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Models;
using BussinessLogic.Services;
using DataAccess.DTOs;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class SystemAccountsController : ODataController
    {
        private readonly IAccountService _accountService;

        public SystemAccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            try
            {
                var accounts = _accountService.GetAccountsQueryable();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving accounts", error = ex.Message });
            }
        }

        [EnableQuery]
        public async Task<IActionResult> Get([FromRoute] short key)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(key);
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

                var createdAccount = await _accountService.CreateAccountAsync(account);
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
                existingAccount.AccountRole = updateDto.AccountRole;

                Console.WriteLine($"Calling UpdateAccountAsync...");
                var updatedAccount = await _accountService.UpdateAccountAsync(existingAccount);
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

                var success = await _accountService.DeleteAccountAsync(key);
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
    }
}