using DataAccess.Models;
using Repositories;

namespace Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _systemAccountRepository;

        public SystemAccountService(ISystemAccountRepository systemAccountRepository)
        {
            _systemAccountRepository = systemAccountRepository;
        }

        public void AddSystemAccount(SystemAccount account)
        {
            _systemAccountRepository.AddSystemAccount(account);
        }

        public void DeleteSystemAccount(short id)
        {
            _systemAccountRepository.DeleteSystemAccount(id);
        }

        public SystemAccount? GetSystemAccountByEmail(string email)
        {
            return _systemAccountRepository.GetSystemAccountByEmail(email);
        }

        public SystemAccount? GetSystemAccountById(short id)
        {
            return _systemAccountRepository.GetSystemAccountById(id);
        }

        public List<SystemAccount> GetSystemAccountByUsername(string keyword)
        {
            return _systemAccountRepository.GetSystemAccountByUsername(keyword);
        }

        public List<SystemAccount> GetSystemAccounts()
        {
            return _systemAccountRepository.GetSystemAccounts();
        }

        public void UpdateSystemAccount(SystemAccount account)
        {
            _systemAccountRepository.UpdateSystemAccount(account);
        }
    }
}
