using DataAccess.Models;
using DataAccess.Data;

namespace Repositories
{
    public interface ISystemAccountRepository
    {
        public  SystemAccount? GetSystemAccountByEmail(string email);
        public List<SystemAccount> GetSystemAccounts();
        public SystemAccount? GetSystemAccountById(short id);
        public void AddSystemAccount(SystemAccount account);
        public void UpdateSystemAccount(SystemAccount account);
        public void DeleteSystemAccount(short id);
        public List<SystemAccount> GetSystemAccountByUsername(string keyword);
    }
}

