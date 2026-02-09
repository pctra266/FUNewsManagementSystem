using DataAccess.Models;

namespace BussinessLogic.Services
{
    public interface IAccountService
    {
        Task<IEnumerable<SystemAccount>> GetAllAccountsAsync();
        Task<SystemAccount?> GetAccountByIdAsync(short id);
        Task<SystemAccount?> GetAccountByEmailAsync(string email);
        Task<SystemAccount?> AuthenticateAsync(string email, string password);
        Task<SystemAccount> CreateAccountAsync(SystemAccount account, short? userId = null);
        Task<SystemAccount> UpdateAccountAsync(SystemAccount account, short? userId = null);
        Task<bool> DeleteAccountAsync(short id, short? userId = null);
        Task<bool> CanDeleteAccountAsync(short id);
        Task<IEnumerable<SystemAccount>> SearchAccountsAsync(string? name = null, string? email = null, int? role = null);
        Task<bool> IsEmailExistAsync(string email, short? excludeId = null);
        Task<bool> ChangePasswordAsync(short accountId, string currentPassword, string newPassword);
        IQueryable<SystemAccount> GetAccountsQueryable();
    }
}