using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISystemAccountService
    {
        public SystemAccount? GetSystemAccountByEmail(string email);
        public List<SystemAccount> GetSystemAccounts();
        public SystemAccount? GetSystemAccountById(short id);
        public void AddSystemAccount(SystemAccount account);
        public void UpdateSystemAccount(SystemAccount account);
        public void DeleteSystemAccount(short id);
        public List<SystemAccount> GetSystemAccountByUsername(string keyword);
    }
}
