using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IAccountService
    {
        // Dùng cho Login Form
        Task<SystemAccount?> LoginAsync(string email, string password);

        // Dùng để lấy toàn bộ danh sách (cho Admin quản lý)
        Task<List<SystemAccount>> GetAccountsAsync();

        // Dùng cho Google Login: Kiểm tra xem email đã tồn tại chưa
        Task<SystemAccount?> GetAccountByEmailAsync(string email);

        // Dùng cho Google Login: Tạo mới user nếu chưa tồn tại
        Task CreateAccountAsync(SystemAccount account);
        Task<SystemAccount?> GetAccountByIdAsync(short id);
        Task DeleteAccountAsync(short id);
        Task UpdateAccountAsync(SystemAccount account);
    }
}
