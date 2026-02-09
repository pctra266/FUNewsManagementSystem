using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;
using BussinessLogic.Services;
using System.Text.Json;

namespace BussinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AccountService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAccountsAsync()
        {
            return await _unitOfWork.AccountRepository.GetAllAsync();
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(short id)
        {
            return await _unitOfWork.AccountRepository.GetByIdAsync(id);
        }

        public async Task<SystemAccount?> GetAccountByEmailAsync(string email)
        {
            return await _unitOfWork.AccountRepository.FindSingleAsync(a => a.AccountEmail == email);
        }

        public async Task<SystemAccount?> AuthenticateAsync(string email, string password)
        {
            var account = await _unitOfWork.AccountRepository
                .FindSingleAsync(a => a.AccountEmail == email && a.AccountPassword == password);
            
            return account;
        }

        public async Task<SystemAccount> CreateAccountAsync(SystemAccount account, short? userId = null)
        {
            // Check if email already exists
            if (await IsEmailExistAsync(account.AccountEmail!))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Generate new ID
            var allAccounts = await _unitOfWork.AccountRepository.GetAllAsync();
            account.AccountId = (short)(allAccounts.Any() ? allAccounts.Max(a => a.AccountId) + 1 : 1);

            await _unitOfWork.AccountRepository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            // Audit Log
            if (userId.HasValue)
            {
                // Mask password before logging
                var accountToLog = new SystemAccount
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    AccountEmail = account.AccountEmail,
                    AccountRole = account.AccountRole,
                    AccountPassword = "***"
                };
                await _auditService.LogAsync(userId.Value, "Create", "SystemAccount", account.AccountId.ToString(), null, accountToLog);
            }
            
            return account;
        }

        public async Task<SystemAccount> UpdateAccountAsync(SystemAccount account, short? userId = null)
        {
            // Check if email already exists for other accounts
            if (await IsEmailExistAsync(account.AccountEmail!, account.AccountId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var existingAccount = await _unitOfWork.AccountRepository.GetByIdAsync(account.AccountId);
            if (existingAccount == null)
            {
                 throw new InvalidOperationException("Account not found");
            }

            // Keep a copy of old values for logging
            var oldAccountState = new SystemAccount
            {
                AccountId = existingAccount.AccountId,
                AccountName = existingAccount.AccountName,
                AccountEmail = existingAccount.AccountEmail,
                AccountRole = existingAccount.AccountRole,
                AccountPassword = "***"
            };

            existingAccount.AccountName = account.AccountName;
            existingAccount.AccountEmail = account.AccountEmail;
            existingAccount.AccountRole = account.AccountRole;
            // Password update is handled separately, or here if passed. 
            // The controller update logic for PUT implies full update, but usually password change is separate.
            // Let's assume standard properties update here.

            _unitOfWork.AccountRepository.Update(existingAccount);
            await _unitOfWork.SaveChangesAsync();

             // Audit Log
            if (userId.HasValue)
            {
                var newAccountState = new SystemAccount
                {
                    AccountId = existingAccount.AccountId,
                    AccountName = existingAccount.AccountName,
                    AccountEmail = existingAccount.AccountEmail,
                    AccountRole = existingAccount.AccountRole,
                    AccountPassword = "***"
                };
                await _auditService.LogAsync(userId.Value, "Update", "SystemAccount", account.AccountId.ToString(), oldAccountState, newAccountState);
            }
            
            return existingAccount;
        }

        public async Task<bool> DeleteAccountAsync(short id, short? userId = null)
        {
            if (!await CanDeleteAccountAsync(id))
            {
                return false;
            }

            var account = await _unitOfWork.AccountRepository.GetByIdAsync(id);
            if (account == null)
            {
                return false;
            }

            // Keep a copy for log
            var accountToLog = new SystemAccount
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole,
                AccountPassword = "***"
            };

            _unitOfWork.AccountRepository.Delete(account);
            await _unitOfWork.SaveChangesAsync();

            // Audit Log
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "Delete", "SystemAccount", id.ToString(), accountToLog, null);
            }
            
            return true;
        }

        public async Task<bool> CanDeleteAccountAsync(short id)
        {
            // Check if account has created any news articles
            return !await _unitOfWork.NewsArticleRepository.ExistsAsync(n => n.CreatedById == id);
        }

        public async Task<IEnumerable<SystemAccount>> SearchAccountsAsync(string? name = null, string? email = null, int? role = null)
        {
            var query = _unitOfWork.AccountRepository.Query();

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

            return await query.ToListAsync();
        }

        public async Task<bool> IsEmailExistAsync(string email, short? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.AccountRepository
                    .ExistsAsync(a => a.AccountEmail == email && a.AccountId != excludeId);
            }
            
            return await _unitOfWork.AccountRepository
                .ExistsAsync(a => a.AccountEmail == email);
        }

        public async Task<bool> ChangePasswordAsync(short accountId, string currentPassword, string newPassword)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            
            if (account == null || account.AccountPassword != currentPassword)
            {
                return false;
            }

            account.AccountPassword = newPassword;
            _unitOfWork.AccountRepository.Update(account);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        public IQueryable<SystemAccount> GetAccountsQueryable()
        {
            return _unitOfWork.AccountRepository.Query();
        }
    }
}