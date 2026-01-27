using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class SystemAccountsController : ControllerBase
    {
        private readonly ISystemAccountRepository _systemAccountRepository;
        private readonly INewsArticleRepository _newsArticleRepository;

        public SystemAccountsController(
            ISystemAccountRepository systemAccountRepository,
            INewsArticleRepository newsArticleRepository)
        {
            _systemAccountRepository = systemAccountRepository;
            _newsArticleRepository = newsArticleRepository;
        }

        // GET: api/SystemAccounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemAccount>>> GetSystemAccounts()
        {
            var accounts = await Task.Run(() => _systemAccountRepository.GetSystemAccounts());
            return Ok(accounts);
        }

        // GET: api/SystemAccounts/Search?keyword=john&role=1
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<SystemAccount>>> SearchAccounts(
            [FromQuery] string? keyword,
            [FromQuery] short? role)
        {
            var accounts = await Task.Run(() => _systemAccountRepository.GetSystemAccounts());
            
            if (!string.IsNullOrEmpty(keyword))
            {
                accounts = accounts.Where(a => 
                    (a.AccountName != null && a.AccountName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (a.AccountEmail != null && a.AccountEmail.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            if (role.HasValue)
            {
                accounts = accounts.Where(a => a.AccountRole == role.Value).ToList();
            }
            
            return Ok(accounts);
        }

        // GET: api/SystemAccounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemAccount>> GetSystemAccount(short id)
        {
            var account = await Task.Run(() => _systemAccountRepository.GetSystemAccountById(id));
            if (account == null) return NotFound();
            return Ok(account);
        }

        // PUT: api/SystemAccounts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystemAccount(short id, SystemAccount systemAccount)
        {
            if (id != systemAccount.AccountId) 
                return BadRequest(new { message = "Account ID mismatch" });
            
            // ✅ Kiểm tra email trùng lặp (trừ chính nó)
            var existingAccount = await Task.Run(() => 
                _systemAccountRepository.GetSystemAccountByEmail(systemAccount.AccountEmail));
            
            if (existingAccount != null && existingAccount.AccountId != id)
            {
                return Conflict(new { message = "Email already exists." });
            }
            
            try
            {
                await Task.Run(() => _systemAccountRepository.UpdateSystemAccount(systemAccount));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating account: {ex.Message}" });
            }
        }

        // POST: api/SystemAccounts
        [HttpPost]
        public async Task<ActionResult<SystemAccount>> PostSystemAccount(SystemAccount systemAccount)
        {
            // ✅ Kiểm tra email đã tồn tại
            var existingAccount = await Task.Run(() => 
                _systemAccountRepository.GetSystemAccountByEmail(systemAccount.AccountEmail));
            
            if (existingAccount != null)
            {
                return Conflict(new { message = "Email already exists." });
            }
            
            try
            {
                await Task.Run(() => _systemAccountRepository.AddSystemAccount(systemAccount));
                return CreatedAtAction("GetSystemAccount", new { id = systemAccount.AccountId }, systemAccount);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating account: {ex.Message}" });
            }
        }

        // DELETE: api/SystemAccounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemAccount(short id)
        {
            var account = await Task.Run(() => _systemAccountRepository.GetSystemAccountById(id));
            if (account == null) 
                return NotFound(new { message = "Account not found." });
            
            // ✅ Kiểm tra account đã tạo articles chưa
            var createdArticles = await Task.Run(() => 
                _newsArticleRepository.GetNewsArticlesByCreatedBy(id));
            
            if (createdArticles.Any())
            {
                return BadRequest(new { 
                    message = "Cannot delete account. This account has created news articles.",
                    articleCount = createdArticles.Count
                });
            }
            
            try
            {
                await Task.Run(() => _systemAccountRepository.DeleteSystemAccount(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting account: {ex.Message}" });
            }
        }

        // PUT: api/SystemAccounts/5/ChangePassword
        [HttpPut("{id}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(short id, [FromBody] PasswordChangeRequest request)
        {
            var account = await Task.Run(() => _systemAccountRepository.GetSystemAccountById(id));
            if (account == null) 
                return NotFound(new { message = "Account not found." });
            
            // ✅ Xác thực mật khẩu hiện tại
            if (account.AccountPassword != request.CurrentPassword)
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }
            
            // Cập nhật mật khẩu mới
            account.AccountPassword = request.NewPassword;
            
            try
            {
                await Task.Run(() => _systemAccountRepository.UpdateSystemAccount(account));
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error changing password: {ex.Message}" });
            }
        }
    }

    // DTO cho thay đổi mật khẩu
    public class PasswordChangeRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
