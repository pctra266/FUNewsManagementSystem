using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Repositories;
using BussinessLogic.Services;

namespace BussinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public async Task<SystemAccount> CreateAccountAsync(SystemAccount account)
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
            
            return account;
        }

        public async Task<SystemAccount> UpdateAccountAsync(SystemAccount account)
        {
            // Check if email already exists for other accounts
            if (await IsEmailExistAsync(account.AccountEmail!, account.AccountId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            _unitOfWork.AccountRepository.Update(account);
            await _unitOfWork.SaveChangesAsync();
            
            return account;
        }

        public async Task<bool> DeleteAccountAsync(short id)
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

            _unitOfWork.AccountRepository.Delete(account);
            await _unitOfWork.SaveChangesAsync();
            
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