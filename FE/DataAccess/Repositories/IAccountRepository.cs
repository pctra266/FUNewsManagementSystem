using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface IAccountRepository
    {
        Task<SystemAccount?> LoginAsync(string email, string password);
        Task<List<SystemAccount>> GetAccountsAsync();
        Task CreateAccountAsync(SystemAccount account);
        Task<SystemAccount?> GetAccountByIdAsync(short id);
        Task DeleteAccountAsync(short id);
        Task UpdateAccountAsync(SystemAccount account);
    }
}
