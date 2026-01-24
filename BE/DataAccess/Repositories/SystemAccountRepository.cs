using DataAccess.Models;
using DataAccess.Data;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class SystemAccountRepository : ISystemAccountRepository
    {
        // 1. Khai báo biến readonly cho Context
        private readonly NewsContext _context;

        // 2. Inject Context qua Constructor
        public SystemAccountRepository(NewsContext context)
        {
            _context = context;
        }

        public void AddSystemAccount(SystemAccount account)
        {
            _context.SystemAccounts.Add(account);
            _context.SaveChanges();
        }

        public void DeleteSystemAccount(short id)
        {
            // Tìm đối tượng trước khi xóa
            var account = _context.SystemAccounts.Find(id);
            if (account != null)
            {
                _context.SystemAccounts.Remove(account);
                _context.SaveChanges();
            }
        }

        public SystemAccount? GetSystemAccountByEmail(string email)
        {
            return _context.SystemAccounts.FirstOrDefault(a => a.AccountEmail == email);
        }

        public SystemAccount? GetSystemAccountById(short id)
        {
            return _context.SystemAccounts.Find(id);
        }

        public List<SystemAccount> GetSystemAccountByUsername(string username)
        {
            // Lưu ý: Logic gốc là tìm chính xác (==), nếu muốn tìm gần đúng thì dùng Contains
            return _context.SystemAccounts
                           .Where(a => a.AccountName == username)
                           .ToList();
        }

        public List<SystemAccount> GetSystemAccounts()
        {
            return _context.SystemAccounts.ToList();
        }

        public void UpdateSystemAccount(SystemAccount account)
        {
            _context.SystemAccounts.Update(account);
            _context.SaveChanges();
        }

        // Bổ sung hàm kiểm tra tồn tại (thường dùng khi Update/Delete)
        public bool CheckSystemAccountExists(short id)
        {
            return _context.SystemAccounts.Any(a => a.AccountId == id);
        }
    }
}