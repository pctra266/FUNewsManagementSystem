using DataAccess.Models;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class AccountService: IAccountService
    {
        private readonly IAccountRepository _accountRepo;

        public AccountService(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public async Task<SystemAccount?> LoginAsync(string email, string password)
        {
            // Cách 1: Nếu API có endpoint Login riêng
            return await _accountRepo.LoginAsync(email, password);

            // Cách 2: Nếu API không có Login, phải get hết về rồi lọc (chỉ dùng khi project nhỏ)
            /*
            var accounts = await _accountRepo.GetAccountsAsync();
            return accounts.FirstOrDefault(a => a.AccountEmail == email && a.AccountPassword == password);
            */
        }

        public async Task<List<SystemAccount>> GetAccountsAsync()
        {
            return await _accountRepo.GetAccountsAsync();
        }

        public async Task<SystemAccount?> GetAccountByEmailAsync(string email)
        {
            // Lấy danh sách từ Repo về và lọc
            var accounts = await _accountRepo.GetAccountsAsync();

            // Trả về user đầu tiên khớp email (không phân biệt hoa thường)
            return accounts.FirstOrDefault(a => a.AccountEmail != null &&
                                                a.AccountEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task CreateAccountAsync(SystemAccount account)
        {
            // LOGIC SINH ID (Vì Database của bạn không tự tăng ID cho bảng Account)
            // Ta lấy danh sách hiện tại để tìm ID lớn nhất rồi + 1
            var accounts = await _accountRepo.GetAccountsAsync();

            short newId = 1;
            if (accounts.Any())
            {
                newId = (short)(accounts.Max(a => a.AccountId) + 1);
            }
            account.AccountId = newId;

            // Đảm bảo các trường bắt buộc có dữ liệu
            if (string.IsNullOrEmpty(account.AccountPassword))
            {
                account.AccountPassword = "@1"; // Mật khẩu mặc định cho Google User
            }

            if (account.AccountRole == 0)
            {
                account.AccountRole = 2; // Mặc định là User thường
            }

            // Gọi Repo để đẩy xuống API
            await _accountRepo.CreateAccountAsync(account);
        }
        public async Task<SystemAccount?> GetAccountByIdAsync(short id)
        {
            return await _accountRepo.GetAccountByIdAsync(id);
        }

        public async Task DeleteAccountAsync(short id)
        {
            // Có thể thêm logic: Không cho phép xóa chính mình (Admin đang login)
            // if (id == currentUserId) throw new Exception("Cannot delete yourself.");

            await _accountRepo.DeleteAccountAsync(id);
        }
        public async Task UpdateAccountAsync(SystemAccount account)
        {
            // Có thể thêm logic: Nếu đổi password thì mã hóa lại, hoặc validation nghiệp vụ
            // Ví dụ: Không cho phép đổi Role của chính mình nếu đang là Admin duy nhất...

            await _accountRepo.UpdateAccountAsync(account);
        }
    }
}
