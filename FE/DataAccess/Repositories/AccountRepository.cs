using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7000/api/SystemAccount"; // ĐỔI PORT CỦA BẠN
        private readonly JsonSerializerOptions _options;
        public AccountRepository()
        {
            _httpClient = new HttpClient();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public async Task<SystemAccount?> LoginAsync(string email, string password)
        {
            // Giả sử API login của bạn là POST /api/SystemAccount/login
            // Hoặc bạn có thể filter từ list accounts nếu API đơn giản
            var allAccounts = await GetAccountsAsync();
            return allAccounts.FirstOrDefault(a => a.AccountEmail == email && a.AccountPassword == password);
        }

        public async Task<List<SystemAccount>> GetAccountsAsync()
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SystemAccount>>(content, _options) ?? new List<SystemAccount>();
        }
        public async Task CreateAccountAsync(SystemAccount account)
        {
            // Gọi API POST để tạo mới
            var json = JsonSerializer.Serialize(account);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Giả sử API tạo user là POST: api/SystemAccount
            var response = await _httpClient.PostAsync(_apiBaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // Ném lỗi hoặc log nếu tạo thất bại
                throw new Exception("Không thể tạo tài khoản mới qua API.");
            }
        }
        public async Task<SystemAccount?> GetAccountByIdAsync(short id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SystemAccount>(content, _options);
        }

        public async Task DeleteAccountAsync(short id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");
            response.EnsureSuccessStatusCode(); // Ném lỗi nếu API trả về khác 2xx
        }
        public async Task UpdateAccountAsync(SystemAccount account)
        {
            var json = JsonSerializer.Serialize(account);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gọi API PUT: api/SystemAccounts/{id}
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{account.AccountId}", content);

            response.EnsureSuccessStatusCode();
        }
    }
}
